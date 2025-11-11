using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;
using Stripe;
using Stripe.Checkout;
using System.Text.Json;

namespace ReacodeApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;
        private readonly string _stripeSecretKey;

        public PaymentController(
            ApplicationDbContext context,
            ISessionService sessionService,
            IConfiguration configuration,
            ILogger<PaymentController> logger)
        {
            _context = context;
            _sessionService = sessionService;
            _configuration = configuration;
            _logger = logger;
            _stripeSecretKey = _configuration["Stripe:SecretKey"] ?? string.Empty;
            
            // Initialize Stripe API key
            if (!string.IsNullOrEmpty(_stripeSecretKey))
            {
                StripeConfiguration.ApiKey = _stripeSecretKey;
            }
        }

        // Create Stripe Checkout Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutDeliveryInfo deliveryInfo)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to continue." });
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            // Validate delivery info
            if (deliveryInfo == null || 
                string.IsNullOrWhiteSpace(deliveryInfo.DeliveryAddress) ||
                string.IsNullOrWhiteSpace(deliveryInfo.DeliveryCity) ||
                string.IsNullOrWhiteSpace(deliveryInfo.DeliveryProvince) ||
                string.IsNullOrWhiteSpace(deliveryInfo.DeliveryPostalCode) ||
                !System.Text.RegularExpressions.Regex.IsMatch(deliveryInfo.DeliveryPostalCode, @"^\d{4}$"))
            {
                return Json(new { success = false, message = "Please fill in all required delivery information correctly." });
            }

            // Get cart items
            var cart = await GetCartFromDatabaseAsync(user.Id);
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            // Get farmer ID from first product
            var firstProduct = await _context.Products.FindAsync(cart.First().ProductId);
            if (firstProduct == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            // Calculate total amount
            var totalAmount = cart.Sum(item => item.TotalPrice);

            // Create order (pending status, will be updated after payment)
            var orderCreatedAt = DateTime.UtcNow;
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                BuyerId = user.Id,
                FarmerId = firstProduct.FarmerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                DeliveryAddress = deliveryInfo.DeliveryAddress,
                DeliveryCity = deliveryInfo.DeliveryCity,
                DeliveryProvince = deliveryInfo.DeliveryProvince,
                DeliveryPostalCode = deliveryInfo.DeliveryPostalCode,
                DeliveryNotes = deliveryInfo.DeliveryNotes,
                PaymentMethod = Models.PaymentMethod.Stripe,
                ResponseDeadline = orderCreatedAt.AddMinutes(30),
                CreatedAt = orderCreatedAt,
                UpdatedAt = orderCreatedAt
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cart)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.TotalPrice,
                        CreatedAt = orderCreatedAt,
                        UpdatedAt = orderCreatedAt
                    };
                    _context.OrderItems.Add(orderItem);
                }
            }
            await _context.SaveChangesAsync();

            // Create Stripe Checkout Session
            // Stripe will automatically append ?session_id={CHECKOUT_SESSION_ID} to the success URL
            var successUrl = Url.Action("PaymentSuccess", "Payment", new { orderId = order.Id }, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.Action("PaymentCancel", "Payment", new { orderId = order.Id }, Request.Scheme);
            
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = cart.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // Convert to cents
                        Currency = "zar",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName,
                            Description = $"{item.Quantity} kg"
                        }
                    },
                    Quantity = item.Quantity
                }).ToList(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.Id.ToString() },
                    { "OrderNumber", order.OrderNumber },
                    { "BuyerId", user.Id.ToString() }
                }
            };

            try
            {
                var service = new Stripe.Checkout.SessionService();
                var session = await service.CreateAsync(options);

                // Store session ID in order's PaymentReference for fallback lookup
                order.PaymentReference = session.Id;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    sessionId = session.Id,
                    url = session.Url
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout session for order {OrderId}", order.Id);
                return Json(new { success = false, message = $"Payment error: {ex.Message}" });
            }
        }

        // Payment Success - Called after successful Stripe payment
        public async Task<IActionResult> PaymentSuccess(int? orderId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return RedirectToAction("Index", "Home");
            }

            // Try to get orderId from query string if not provided
            if (!orderId.HasValue)
            {
                var orderIdStr = Request.Query["orderId"].ToString();
                if (!string.IsNullOrEmpty(orderIdStr) && int.TryParse(orderIdStr, out var parsedOrderId))
                {
                    orderId = parsedOrderId;
                }
            }

            Order? order = null;
            
            if (orderId.HasValue)
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.BuyerId == user.Id);
            }

            // If order not found by ID, try to find by session_id from Stripe metadata
            if (order == null)
            {
                var sessionIdFromQuery = Request.Query["session_id"].ToString();
                if (!string.IsNullOrEmpty(sessionIdFromQuery))
                {
                    try
                    {
                        var service = new Stripe.Checkout.SessionService();
                        var session = await service.GetAsync(sessionIdFromQuery);
                        
                        if (session.Metadata != null && session.Metadata.ContainsKey("OrderId"))
                        {
                            var metadataOrderId = int.Parse(session.Metadata["OrderId"]);
                            order = await _context.Orders
                                .FirstOrDefaultAsync(o => o.Id == metadataOrderId && o.BuyerId == user.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving order from Stripe session metadata");
                    }
                }
            }

            if (order == null)
            {
                TempData["Error"] = "Order not found. Please check your order history or contact support.";
                return RedirectToAction("OrdersHistory", "Buyer");
            }

            // Check if order is already processed
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Success"] = $"Order #{order.OrderNumber} has already been processed.";
                return RedirectToAction("OrdersHistory", "Buyer");
            }

            // Get the Stripe session ID from query string, or from order's PaymentReference as fallback
            var sessionId = Request.Query["session_id"].ToString();
            
            // If session_id is not in query string, try to get it from order's PaymentReference
            if (string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(order.PaymentReference))
            {
                sessionId = order.PaymentReference;
            }
            
            // If still no session_id, try to get it from existing payment transaction
            if (string.IsNullOrEmpty(sessionId))
            {
                var existingTransaction = await _context.PaymentTransactions
                    .FirstOrDefaultAsync(pt => pt.OrderId == order.Id && pt.PaymentGateway == "Stripe");
                
                if (existingTransaction != null && existingTransaction.Status == "succeeded")
                {
                    // Payment already processed - but order should remain Pending for farmer to accept/decline
                    if (order.Status == OrderStatus.Pending)
                    {
                        TempData["Success"] = $"Payment successful! Order #{order.OrderNumber} is waiting for farmer confirmation.";
                        return RedirectToAction("OrdersHistory", "Buyer");
                    }
                    else
                    {
                        TempData["Success"] = $"Payment successful! Order #{order.OrderNumber} has been placed.";
                        return RedirectToAction("OrdersHistory", "Buyer");
                    }
                }
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var service = new Stripe.Checkout.SessionService();
                    var session = await service.GetAsync(sessionId);

                    // Check if payment was successful
                    if (session.PaymentStatus == "paid" || session.Status == "complete")
                    {
                        // Check if already processed
                        var existingTransaction = await _context.PaymentTransactions
                            .FirstOrDefaultAsync(pt => pt.OrderId == order.Id && pt.TransactionId == (session.PaymentIntentId ?? sessionId));

                        if (existingTransaction == null)
                        {
                            // Keep order status as Pending so farmer can accept/decline
                            // Payment is successful, but order needs farmer confirmation
                            order.Status = OrderStatus.Pending;
                            order.PaymentReference = session.PaymentIntentId ?? sessionId;
                            order.UpdatedAt = DateTime.UtcNow;

                            // Create payment transaction record
                            var transaction = new PaymentTransaction
                            {
                                OrderId = order.Id,
                                PaymentGateway = "Stripe",
                                TransactionId = session.PaymentIntentId ?? sessionId,
                                Amount = order.TotalAmount,
                                Currency = "ZAR",
                                Status = "succeeded",
                                RawResponse = JsonSerializer.Serialize(session),
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.PaymentTransactions.Add(transaction);

                            // Clear cart
                            await ClearCartAsync(user.Id);

                            // Create notification for farmer to accept or decline
                            var notification = new Notification
                            {
                                UserId = order.FarmerId,
                                Title = "New Order Received - Payment Confirmed",
                                Message = $"You have received a new order #{order.OrderNumber} for R {order.TotalAmount:N2}. Payment has been confirmed. Please accept or decline this order within 30 minutes.",
                                Type = NotificationType.Order,
                                IsRead = false,
                                RelatedEntityId = order.Id,
                                RelatedEntityType = "Order",
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);

                            await _context.SaveChangesAsync();

                            TempData["Success"] = $"Payment successful! Order #{order.OrderNumber} has been placed.";
                            return RedirectToAction("OrdersHistory", "Buyer");
                        }
                        else
                        {
                            // Already processed
                            TempData["Success"] = $"Payment successful! Order #{order.OrderNumber} has been placed.";
                            return RedirectToAction("OrdersHistory", "Buyer");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Payment session {SessionId} for order {OrderId} has status: {PaymentStatus}, {Status}", 
                            sessionId, order.Id, session.PaymentStatus, session.Status);
                        TempData["Error"] = $"Payment status: {session.PaymentStatus}. Please contact support if payment was processed.";
                        return RedirectToAction("OrdersHistory", "Buyer");
                    }
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe error verifying payment for order {OrderId}, session {SessionId}", order.Id, sessionId);
                    TempData["Error"] = $"Payment verification error: {ex.Message}. Please contact support if payment was processed.";
                    return RedirectToAction("OrdersHistory", "Buyer");
                }
            }
            else
            {
                _logger.LogWarning("No session_id found in query string for order {OrderId}", order?.Id ?? 0);
                // Try to verify payment by checking Stripe for sessions related to this order
                try
                {
                    var service = new Stripe.Checkout.SessionService();
                    var options = new Stripe.Checkout.SessionListOptions
                    {
                        Limit = 10
                    };
                    var sessions = await service.ListAsync(options);
                    
                    // Find session with matching order metadata
                    var matchingSession = sessions.Data.FirstOrDefault(s => 
                        order != null &&
                        s.Metadata != null && 
                        s.Metadata.ContainsKey("OrderId") && 
                        s.Metadata.TryGetValue("OrderId", out var orderIdValue) &&
                        orderIdValue != null &&
                        orderIdValue == order.Id.ToString() &&
                        (s.PaymentStatus == "paid" || s.Status == "complete"));

                    if (matchingSession != null && order != null)
                    {
                        // Payment found and verified - Keep order as Pending for farmer to accept/decline
                        order.Status = OrderStatus.Pending;
                        var transactionId = matchingSession.PaymentIntentId ?? matchingSession.Id ?? string.Empty;
                        order.PaymentReference = transactionId;
                        order.UpdatedAt = DateTime.UtcNow;

                        // Check if transaction already exists
                        var existingTransaction = await _context.PaymentTransactions
                            .FirstOrDefaultAsync(pt => pt.OrderId == order.Id && pt.TransactionId == transactionId);

                        if (existingTransaction == null)
                        {
                            var transaction = new PaymentTransaction
                            {
                                OrderId = order.Id,
                                PaymentGateway = "Stripe",
                                TransactionId = transactionId,
                                Amount = order.TotalAmount,
                                Currency = "ZAR",
                                Status = "succeeded",
                                RawResponse = JsonSerializer.Serialize(matchingSession),
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.PaymentTransactions.Add(transaction);
                        }

                        await ClearCartAsync(user.Id);

                        // Create notification for farmer to accept or decline
                        var notification = new Notification
                        {
                            UserId = order.FarmerId,
                            Title = "New Order Received - Payment Confirmed",
                            Message = $"You have received a new order #{order.OrderNumber} for R {order.TotalAmount:N2}. Payment has been confirmed. Please accept or decline this order within 30 minutes.",
                            Type = NotificationType.Order,
                            IsRead = false,
                            RelatedEntityId = order.Id,
                            RelatedEntityType = "Order",
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);

                        await _context.SaveChangesAsync();

                        TempData["Success"] = $"Payment successful! Order #{order.OrderNumber} has been placed.";
                        return RedirectToAction("OrdersHistory", "Buyer");
                    }
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Error searching for Stripe session for order {OrderId}", orderId);
                }

                TempData["Error"] = "Payment verification failed. Please contact support if payment was processed.";
                return RedirectToAction("OrdersHistory", "Buyer");
            }
        }

        // Payment Cancel
        public async Task<IActionResult> PaymentCancel(int orderId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == user.Id);

            if (order != null && order.Status == OrderStatus.Pending)
            {
                // Optionally cancel the order or keep it pending
                // For now, we'll keep it pending so user can retry payment
            }

            TempData["Error"] = "Payment was cancelled. You can try again from your order history.";
            return RedirectToAction("OrdersHistory", "Buyer");
        }

        // Stripe Webhook - Handle payment events
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            // Webhook secret can be added here for production verification
            // const string secret = _configuration["Stripe:WebhookSecret"] ?? "";
            // var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], secret);

            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                
                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session != null && session.Metadata != null && session.Metadata.ContainsKey("OrderId"))
                    {
                        var orderId = int.Parse(session.Metadata["OrderId"]);
                        var order = await _context.Orders.FindAsync(orderId);
                        
                        if (order != null && order.Status == OrderStatus.Pending)
                        {
                            order.Status = OrderStatus.Accepted;
                            order.PaymentReference = session.PaymentIntentId ?? session.Id;
                            order.UpdatedAt = DateTime.UtcNow;

                            // Create payment transaction record
                            var transaction = new PaymentTransaction
                            {
                                OrderId = order.Id,
                                PaymentGateway = "Stripe",
                                TransactionId = session.PaymentIntentId ?? session.Id,
                                Amount = order.TotalAmount,
                                Currency = "ZAR",
                                Status = "succeeded",
                                RawResponse = json,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.PaymentTransactions.Add(transaction);

                            // Create notification for farmer
                            var notification = new Notification
                            {
                                UserId = order.FarmerId,
                                Title = "New Order Received",
                                Message = $"You have received a new order #{order.OrderNumber} for R {order.TotalAmount:N2}. Please review and respond within 30 minutes.",
                                Type = NotificationType.Order,
                                IsRead = false,
                                RelatedEntityId = order.Id,
                                RelatedEntityType = "Order",
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Notifications.Add(notification);

                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }
        }

        // Helper Methods
        private async Task<List<CartItem>> GetCartFromDatabaseAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();

            return cartItems.Select(c => new CartItem
            {
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                Price = c.Product.PricePerKg,
                Quantity = c.Quantity,
                ImageUrl = c.Product.ImageUrl
            }).ToList();
        }

        private async Task ClearCartAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();
            
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            
            HttpContext.Session.Remove("Cart");
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }

    // DTO for checkout delivery info
    public class CheckoutDeliveryInfo
    {
        public string DeliveryAddress { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryProvince { get; set; } = string.Empty;
        public string DeliveryPostalCode { get; set; } = string.Empty;
        public string? DeliveryNotes { get; set; }
    }
}

