using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ReacodeApp.Data;
using ReacodeApp.Models;

namespace ReacodeApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BuyersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BuyersController> _logger;

        public BuyersController(ApplicationDbContext context, ILogger<BuyersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        private async Task<bool> IsBuyer(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.Role == UserRole.Buyer;
        }

        private async Task<List<CartItemDto>> GetCartFromDatabaseAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                    .ThenInclude(p => p.Category)
                .Include(c => c.Product)
                    .ThenInclude(p => p.Farmer)
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();

            return cartItems.Select(c => new CartItemDto
            {
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                Price = c.Product.PricePerKg,
                Quantity = c.Quantity,
                ImageUrl = c.Product.ImageUrl,
                CategoryName = c.Product.Category.Name,
                FarmerName = $"{c.Product.Farmer.FirstName} {c.Product.Farmer.LastName}",
                TotalPrice = c.Product.PricePerKg * c.Quantity
            }).ToList();
        }

        private async Task ClearCartAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();
            
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get buyer dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<BuyerDashboardDto>> GetDashboard()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access this endpoint");
            }

            var totalOrders = await _context.Orders.CountAsync(o => o.BuyerId == userId);
            var pendingOrders = await _context.Orders.CountAsync(o => o.BuyerId == userId && o.Status == OrderStatus.Pending);
            var deliveredOrders = await _context.Orders.CountAsync(o => o.BuyerId == userId && o.Status == OrderStatus.Delivered);
            var totalSpent = await _context.Orders
                .Where(o => o.BuyerId == userId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentOrders = await _context.Orders
                .Include(o => o.Farmer)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    FarmerName = $"{o.Farmer.FirstName} {o.Farmer.LastName}",
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(new BuyerDashboardDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                DeliveredOrders = deliveredOrders,
                TotalSpent = totalSpent,
                RecentOrders = recentOrders
            });
        }

        /// <summary>
        /// Browse products with filters (public endpoint, but can be used by buyers)
        /// </summary>
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string? searchTerm,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Where(p => p.IsAvailable && p.Status == ProductStatus.Approved && p.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PricePerKg >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PricePerKg <= maxPrice.Value);
            }

            query = sortBy switch
            {
                "price_low" => query.OrderBy(p => p.PricePerKg),
                "price_high" => query.OrderByDescending(p => p.PricePerKg),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    PricePerKg = p.PricePerKg,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    Location = p.Location,
                    HarvestDate = p.HarvestDate,
                    ExpiryDate = p.ExpiryDate,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    FarmerId = p.FarmerId,
                    FarmerName = $"{p.Farmer.FirstName} {p.Farmer.LastName}",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// Get product details with reviews
        /// </summary>
        [HttpGet("products/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDetailsDto>> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Buyer)
                .Where(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            var averageRating = product.Reviews.Any() 
                ? product.Reviews.Average(r => r.Rating) 
                : 0;

            var reviews = product.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    BuyerName = $"{r.Buyer.FirstName} {r.Buyer.LastName}",
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            var productDto = new ProductDetailsDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PricePerKg = product.PricePerKg,
                AvailableQuantity = product.AvailableQuantity,
                ImageUrl = product.ImageUrl,
                Location = product.Location,
                HarvestDate = product.HarvestDate,
                ExpiryDate = product.ExpiryDate,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                FarmerId = product.FarmerId,
                FarmerName = $"{product.Farmer.FirstName} {product.Farmer.LastName}",
                AverageRating = (double)averageRating,
                ReviewCount = product.Reviews.Count,
                Reviews = reviews,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt
            };

            return Ok(productDto);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconUrl = c.IconUrl
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Add product to cart
        /// </summary>
        [HttpPost("cart")]
        public async Task<ActionResult<CartResponseDto>> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can add to cart");
            }

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null || !product.IsAvailable)
            {
                return BadRequest(new { message = "Product not available" });
            }

            if (request.Quantity <= 0 || request.Quantity > product.AvailableQuantity)
            {
                return BadRequest(new { message = "Invalid quantity" });
            }

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);
            
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Carts.Add(cartItem);
            }
            
            await _context.SaveChangesAsync();

            var cart = await GetCartFromDatabaseAsync(userId);
            var cartCount = cart.Sum(item => item.Quantity);
            var totalAmount = cart.Sum(item => item.TotalPrice);

            return Ok(new CartResponseDto
            {
                Success = true,
                Message = "Added to cart",
                CartCount = cartCount,
                TotalAmount = totalAmount
            });
        }

        /// <summary>
        /// Get cart items
        /// </summary>
        [HttpGet("cart")]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access cart");
            }

            var cartItems = await GetCartFromDatabaseAsync(userId);
            var totalAmount = cartItems.Sum(item => item.TotalPrice);
            var cartCount = cartItems.Sum(item => item.Quantity);

            return Ok(new CartDto
            {
                Items = cartItems,
                TotalAmount = totalAmount,
                ItemCount = cartCount
            });
        }

        /// <summary>
        /// Get cart count
        /// </summary>
        [HttpGet("cart/count")]
        public async Task<ActionResult<CartCountDto>> GetCartCount()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access cart");
            }

            var cartItems = await GetCartFromDatabaseAsync(userId);
            var count = cartItems.Sum(item => item.Quantity);

            return Ok(new CartCountDto { Success = true, Count = count });
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("cart/{productId}")]
        public async Task<ActionResult<CartResponseDto>> UpdateCartItem(int productId, [FromBody] UpdateCartItemRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can update cart");
            }

            var cartItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
            {
                return NotFound(new { message = "Cart item not found" });
            }

            if (request.Quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
            }
            else
            {
                if (request.Quantity > cartItem.Product.AvailableQuantity)
                {
                    return BadRequest(new { message = "Quantity exceeds available stock" });
                }
                cartItem.Quantity = request.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var cart = await GetCartFromDatabaseAsync(userId);
            var cartCount = cart.Sum(item => item.Quantity);
            var totalAmount = cart.Sum(item => item.TotalPrice);

            return Ok(new CartResponseDto
            {
                Success = true,
                Message = "Cart updated",
                CartCount = cartCount,
                TotalAmount = totalAmount
            });
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("cart/{productId}")]
        public async Task<ActionResult<CartResponseDto>> RemoveFromCart(int productId)
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can remove from cart");
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            var cart = await GetCartFromDatabaseAsync(userId);
            var cartCount = cart.Sum(item => item.Quantity);
            var totalAmount = cart.Sum(item => item.TotalPrice);

            return Ok(new CartResponseDto
            {
                Success = true,
                Message = "Item removed from cart",
                CartCount = cartCount,
                TotalAmount = totalAmount
            });
        }

        /// <summary>
        /// Get checkout information
        /// </summary>
        [HttpGet("checkout")]
        public async Task<ActionResult<CheckoutDto>> GetCheckout()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access checkout");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var cartItems = await GetCartFromDatabaseAsync(userId);
            if (!cartItems.Any())
            {
                return BadRequest(new { message = "Cart is empty" });
            }

            var totalAmount = cartItems.Sum(item => item.TotalPrice);

            return Ok(new CheckoutDto
            {
                CartItems = cartItems,
                TotalAmount = totalAmount,
                Buyer = new BuyerInfoDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    City = user.City,
                    Province = user.Province,
                    PostalCode = user.PostalCode
                }
            });
        }

        /// <summary>
        /// Process checkout and create order
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<OrderDto>> ProcessCheckout([FromBody] ProcessCheckoutRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can process checkout");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { field = x.Key, errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            var cartItems = await GetCartFromDatabaseAsync(userId);
            if (!cartItems.Any())
            {
                return BadRequest(new { message = "Cart is empty" });
            }

            // Get farmer ID from first product
            var firstProduct = await _context.Products.FindAsync(cartItems.First().ProductId);
            if (firstProduct == null)
            {
                return BadRequest(new { message = "Product not found" });
            }

            // Create order
            var orderCreatedAt = DateTime.UtcNow;
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                BuyerId = userId,
                FarmerId = firstProduct.FarmerId,
                TotalAmount = cartItems.Sum(item => item.TotalPrice),
                Status = OrderStatus.Pending,
                DeliveryAddress = request.DeliveryAddress,
                DeliveryCity = request.DeliveryCity,
                DeliveryProvince = request.DeliveryProvince,
                DeliveryPostalCode = request.DeliveryPostalCode,
                DeliveryNotes = request.DeliveryNotes,
                PaymentMethod = ParsePaymentMethod(request.PaymentMethod),
                ResponseDeadline = orderCreatedAt.AddMinutes(30),
                CreatedAt = orderCreatedAt,
                UpdatedAt = orderCreatedAt
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.TotalPrice,
                        CreatedAt = orderCreatedAt,
                        UpdatedAt = orderCreatedAt
                    };

                    _context.OrderItems.Add(orderItem);

                    // Update product quantity
                    product.AvailableQuantity -= cartItem.Quantity;
                    if (product.AvailableQuantity <= 0)
                    {
                        product.IsAvailable = false;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Clear cart
            await ClearCartAsync(userId);

            // Create notification for farmer
            var user = await _context.Users.FindAsync(userId);
            var productNames = string.Join(", ", cartItems.Take(3).Select(c => c.ProductName));
            if (cartItems.Count > 3)
            {
                productNames += $" and {cartItems.Count - 3} more";
            }

            var notification = new Notification
            {
                UserId = order.FarmerId,
                Title = "New Order Received - Action Required",
                Message = $"You have received a new order #{order.OrderNumber} from {user?.FirstName} {user?.LastName} for {productNames}. Total: R{order.TotalAmount:N2}. Please accept or reject within 30 minutes.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Load order with related data for response
            await _context.Entry(order)
                .Reference(o => o.Farmer)
                .LoadAsync();
            await _context.Entry(order)
                .Collection(o => o.OrderItems)
                .Query()
                .Include(oi => oi.Product)
                .LoadAsync();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                DeliveryAddress = order.DeliveryAddress,
                DeliveryCity = order.DeliveryCity,
                DeliveryProvince = order.DeliveryProvince,
                DeliveryPostalCode = order.DeliveryPostalCode,
                DeliveryNotes = order.DeliveryNotes,
                PaymentMethod = order.PaymentMethod.ToString(),
                ResponseDeadline = order.ResponseDeadline,
                FarmerId = order.FarmerId,
                FarmerName = $"{order.Farmer.FirstName} {order.Farmer.LastName}",
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt ?? order.CreatedAt
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }

        /// <summary>
        /// Get all orders for the buyer
        /// </summary>
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access orders");
            }

            var orders = await _context.Orders
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryCity = o.DeliveryCity,
                    DeliveryProvince = o.DeliveryProvince,
                    DeliveryPostalCode = o.DeliveryPostalCode,
                    DeliveryNotes = o.DeliveryNotes,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    TrackingNumber = o.TrackingNumber,
                    ShippedDate = o.ShippedDate,
                    ArrivedDate = o.ArrivedDate,
                    DeliveredDate = o.DeliveredDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    ResponseDeadline = o.ResponseDeadline,
                    RejectionReason = o.RejectionReason,
                    FarmerId = o.FarmerId,
                    FarmerName = $"{o.Farmer.FirstName} {o.Farmer.LastName}",
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt ?? o.CreatedAt
                })
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Get a specific order by ID
        /// </summary>
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access orders");
            }

            var order = await _context.Orders
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id && o.BuyerId == userId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryCity = o.DeliveryCity,
                    DeliveryProvince = o.DeliveryProvince,
                    DeliveryPostalCode = o.DeliveryPostalCode,
                    DeliveryNotes = o.DeliveryNotes,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    TrackingNumber = o.TrackingNumber,
                    ShippedDate = o.ShippedDate,
                    ArrivedDate = o.ArrivedDate,
                    DeliveredDate = o.DeliveredDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    ResponseDeadline = o.ResponseDeadline,
                    RejectionReason = o.RejectionReason,
                    FarmerId = o.FarmerId,
                    FarmerName = $"{o.Farmer.FirstName} {o.Farmer.LastName}",
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt ?? o.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }

        /// <summary>
        /// Confirm delivery and rate farmer
        /// </summary>
        [HttpPost("orders/{id}/confirm-delivery")]
        public async Task<IActionResult> ConfirmDelivery(int id, [FromBody] ConfirmDeliveryRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can confirm delivery");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { message = "Rating must be between 1 and 5" });
            }

            var order = await _context.Orders
                .Include(o => o.Farmer)
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status != OrderStatus.Arrived)
            {
                return BadRequest(new { message = "Order is not ready for delivery confirmation" });
            }

            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;
            order.IsDeliveryConfirmed = true;
            order.UpdatedAt = DateTime.UtcNow;

            // Save or update farmer rating
            var existingRating = await _context.FarmerRatings
                .FirstOrDefaultAsync(r => r.OrderId == order.Id && r.BuyerId == userId);

            if (existingRating != null)
            {
                existingRating.Rating = request.Rating;
                existingRating.Comment = request.Comment;
                existingRating.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var farmerRating = new FarmerRating
                {
                    FarmerId = order.FarmerId,
                    BuyerId = userId,
                    OrderId = order.Id,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.FarmerRatings.Add(farmerRating);
            }

            var notification = new Notification
            {
                UserId = order.FarmerId,
                Title = "Order Delivered",
                Message = $"Order #{order.OrderNumber} has been confirmed as delivered by the buyer. Rating: {request.Rating}/5 stars.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Delivery confirmed successfully", orderId = order.Id });
        }

        /// <summary>
        /// Add product to favorites
        /// </summary>
        [HttpPost("favorites/{productId}")]
        public async Task<IActionResult> AddToFavorites(int productId)
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can add favorites");
            }

            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFavorite != null)
            {
                return BadRequest(new { message = "Product already in favorites" });
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites" });
        }

        /// <summary>
        /// Remove product from favorites
        /// </summary>
        [HttpDelete("favorites/{productId}")]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can remove favorites");
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Removed from favorites" });
        }

        /// <summary>
        /// Get all favorite products
        /// </summary>
        [HttpGet("favorites")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetFavorites()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access favorites");
            }

            var favorites = await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Farmer)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new ProductDto
                {
                    Id = f.Product.Id,
                    Name = f.Product.Name,
                    Description = f.Product.Description,
                    PricePerKg = f.Product.PricePerKg,
                    AvailableQuantity = f.Product.AvailableQuantity,
                    ImageUrl = f.Product.ImageUrl,
                    Location = f.Product.Location,
                    HarvestDate = f.Product.HarvestDate,
                    ExpiryDate = f.Product.ExpiryDate,
                    CategoryId = f.Product.CategoryId,
                    CategoryName = f.Product.Category.Name,
                    FarmerId = f.Product.FarmerId,
                    FarmerName = $"{f.Product.Farmer.FirstName} {f.Product.Farmer.LastName}",
                    CreatedAt = f.Product.CreatedAt,
                    UpdatedAt = f.Product.UpdatedAt ?? f.Product.CreatedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        /// <summary>
        /// Submit a product review
        /// </summary>
        [HttpPost("products/{productId}/reviews")]
        public async Task<IActionResult> SubmitReview(int productId, [FromBody] SubmitReviewRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can submit reviews");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { message = "Rating must be between 1 and 5" });
            }

            // Check if user has purchased this product
            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId && oi.Order.BuyerId == userId && oi.Order.Status == OrderStatus.Delivered);

            if (!hasPurchased)
            {
                return Forbid("You can only review products you have purchased");
            }

            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.BuyerId == userId);

            if (existingReview != null)
            {
                existingReview.Rating = request.Rating;
                existingReview.Comment = request.Comment;
                existingReview.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var review = new ProductReview
                {
                    ProductId = productId,
                    BuyerId = userId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ProductReviews.Add(review);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Review submitted successfully" });
        }

        /// <summary>
        /// Get buyer profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<BuyerProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can access profile");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new BuyerProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                ProfileImage = user.ProfileImage
            });
        }

        /// <summary>
        /// Update buyer profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<BuyerProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsBuyer(userId))
            {
                return Forbid("Only buyers can update profile");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.City = request.City;
            user.Province = request.Province;
            user.PostalCode = request.PostalCode;
            user.ProfileImage = request.ProfileImage;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new BuyerProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                ProfileImage = user.ProfileImage
            });
        }

        // Helper methods
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        private PaymentMethod ParsePaymentMethod(string? method)
        {
            if (string.IsNullOrEmpty(method))
                return PaymentMethod.CashOnDelivery;

            return Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out var result)
                ? result
                : PaymentMethod.CashOnDelivery;
        }
    }

    // DTOs for Buyers API
    public class BuyerDashboardDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; } = new();
    }

    public class ProductDetailsDto : ProductDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FarmerName { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
    }

    public class CartCountDto
    {
        public bool Success { get; set; } = true;
        public int Count { get; set; }
    }

    public class CartResponseDto
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public int CartCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class AddToCartRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }
    }

    public class CheckoutDto
    {
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public BuyerInfoDto Buyer { get; set; } = new();
    }

    public class BuyerInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
    }

    public class ProcessCheckoutRequest
    {
        [Required(ErrorMessage = "Delivery address is required")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string DeliveryCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        public string DeliveryProvince { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Postal code must be exactly 4 digits")]
        public string DeliveryPostalCode { get; set; } = string.Empty;

        public string? DeliveryNotes { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }

    public class ConfirmDeliveryRequest
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class SubmitReviewRequest
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }

    public class BuyerProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? ProfileImage { get; set; }
    }
}

