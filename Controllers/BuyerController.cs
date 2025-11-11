using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;
using System.Text.Json;

namespace ReacodeApp.Controllers
{
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;

        public BuyerController(ApplicationDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // Buyer Dashboard - Overview of purchases, favorites, suggestions
        public async Task<IActionResult> Dashboard()
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

            var buyerId = user.Id;

            // Get buyer statistics
            var totalOrders = await _context.Orders.CountAsync(o => o.BuyerId == buyerId);
            var pendingOrders = await _context.Orders.CountAsync(o => o.BuyerId == buyerId && o.Status == OrderStatus.Pending);
            var deliveredOrders = await _context.Orders.CountAsync(o => o.BuyerId == buyerId && o.Status == OrderStatus.Delivered);
            var totalSpent = await _context.Orders
                .Where(o => o.BuyerId == buyerId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Farmer)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get favorite products (most ordered)
            var favoriteProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Farmer)
                .Where(oi => oi.Order.BuyerId == buyerId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { Product = g.First().Product, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .Take(3)
                .Select(x => x.Product)
                .ToListAsync();

            // Get suggested products (from categories of purchased products)
            var purchasedCategories = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Order.BuyerId == buyerId)
                .Select(oi => oi.Product.CategoryId)
                .Distinct()
                .ToListAsync();

            var suggestedProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Where(p => p.IsAvailable && p.Status == ProductStatus.Approved && 
                           purchasedCategories.Contains(p.CategoryId) && 
                           p.FarmerId != buyerId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Get recent notifications
            var notifications = await _context.Notifications
                .Where(n => n.UserId == buyerId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new BuyerDashboardViewModel
            {
                Buyer = user,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                DeliveredOrders = deliveredOrders,
                TotalSpent = totalSpent,
                RecentOrders = recentOrders,
                FavoriteProducts = favoriteProducts,
                SuggestedProducts = suggestedProducts,
                Notifications = notifications
            };

            return View(viewModel);
        }

        // Shop / Browse Products - Main store page
        public async Task<IActionResult> Shop(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Where(p => p.IsAvailable && p.Status == ProductStatus.Approved);

            // Apply filters
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

            // Apply sorting
            query = sortBy switch
            {
                "price_low" => query.OrderBy(p => p.PricePerKg),
                "price_high" => query.OrderByDescending(p => p.PricePerKg),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await query.ToListAsync();
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            var viewModel = new ShopIndexViewModel
            {
                Products = products,
                Categories = categories,
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return View(viewModel);
        }

        // Product Details - View individual product
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Buyer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Get related products from same category
            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Farmer)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsAvailable && p.Status == ProductStatus.Approved)
                .Take(4)
                .ToListAsync();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = product.Reviews.Count
            };

            return View(viewModel);
        }

        // Add to Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please login to add items to cart" });
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable)
            {
                return Json(new { success = false, message = "Product not available" });
            }

            if (quantity <= 0 || quantity > product.AvailableQuantity)
            {
                return Json(new { success = false, message = "Invalid quantity" });
            }

            // Save to database for logged-in users
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId);
            
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = user.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Carts.Add(cartItem);
            }
            
            await _context.SaveChangesAsync();

            // Also update session cart for consistency
            var cart = await GetCartFromDatabaseAsync(user.Id);
            SaveCartToSession(cart);

            var cartCount = cart.Sum(item => item.Quantity);
            return Json(new { success = true, message = "Added to cart", cartCount = cartCount });
        }

        // Get Cart Count
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, count = 0 });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { success = false, count = 0 });
            }

            var cart = await GetCartFromDatabaseAsync(user.Id);
            var count = cart.Sum(item => item.Quantity);
            
            return Json(new { success = true, count = count });
        }

        // Cart Page
        public async Task<IActionResult> Cart()
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

            // Load cart from database for logged-in users
            var cart = await GetCartFromDatabaseAsync(user.Id);
            
            // Update session cart for consistency
            SaveCartToSession(cart);
            
            return View(cart);
        }

        // Update Cart Item Quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please login" });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId);
            
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Item not found in cart" });
            }

            if (quantity <= 0)
            {
                _context.Carts.Remove(cartItem);
            }
            else
            {
                // Check product availability
                var product = await _context.Products.FindAsync(productId);
                if (product == null || quantity > product.AvailableQuantity)
                {
                    return Json(new { success = false, message = "Invalid quantity" });
                }
                
                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Update session cart
            var cart = await GetCartFromDatabaseAsync(user.Id);
            SaveCartToSession(cart);

            return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity), total = cart.Sum(item => item.TotalPrice) });
        }

        // Remove from Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please login" });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == productId);
            
            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            // Update session cart
            var cart = await GetCartFromDatabaseAsync(user.Id);
            SaveCartToSession(cart);

            return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity), total = cart.Sum(item => item.TotalPrice) });
        }

        // Checkout Page
        public async Task<IActionResult> Checkout()
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

            var cart = await GetCartFromDatabaseAsync(user.Id);
            if (!cart.Any())
            {
                return RedirectToAction("Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                Buyer = user,
                CartItems = cart,
                DeliveryAddress = user.Address ?? "",
                DeliveryCity = user.City ?? "",
                DeliveryProvince = user.Province ?? "",
                DeliveryPostalCode = user.PostalCode ?? "",
                PaymentMethod = PaymentMethod.CashOnDelivery
            };

            return View(viewModel);
        }

        // Process Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout([Bind("DeliveryAddress,DeliveryCity,DeliveryProvince,DeliveryPostalCode,DeliveryNotes,PaymentMethod,CardNumber,CardHolderName,CardExpiryMonth,CardExpiryYear,CardCVV,BankName,AccountNumber,AccountHolderName,ReferenceNumber,WalletType,WalletPhoneNumber,WalletPIN")] CheckoutViewModel model)
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

            var cart = await GetCartFromDatabaseAsync(user.Id);
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty. Please add items before placing an order.";
                return RedirectToAction("Cart");
            }

            // Validate required fields - check if model is null or fields are empty
            if (model == null)
            {
                TempData["Error"] = "Invalid form data. Please try again.";
                return RedirectToAction("Checkout");
            }

            bool hasErrors = false;
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                ModelState.AddModelError("DeliveryAddress", "Delivery address is required");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(model.DeliveryCity))
            {
                ModelState.AddModelError("DeliveryCity", "City is required");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(model.DeliveryProvince))
            {
                ModelState.AddModelError("DeliveryProvince", "Province is required");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(model.DeliveryPostalCode))
            {
                ModelState.AddModelError("DeliveryPostalCode", "Postal code is required");
                hasErrors = true;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.DeliveryPostalCode, @"^\d{4}$"))
            {
                ModelState.AddModelError("DeliveryPostalCode", "Postal code must be exactly 4 digits");
                hasErrors = true;
            }

            // Clear any validation errors for Buyer object (we don't bind it from form)
            ModelState.Remove("Buyer");
            ModelState.Remove("Buyer.Email");
            ModelState.Remove("Buyer.PasswordHash");
            ModelState.Remove("Buyer.FirstName");
            ModelState.Remove("Buyer.LastName");
            ModelState.Remove("CartItems");
            ModelState.Remove("TotalAmount");

            // If validation fails, return to Checkout view with model to preserve input
            if (hasErrors || !ModelState.IsValid)
            {
                // Repopulate cart items for the view
                var cartItems = new List<CartItem>();
                foreach (var item in cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        cartItems.Add(new CartItem
                        {
                            ProductId = item.ProductId,
                            ProductName = product.Name,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            ImageUrl = product.ImageUrl
                        });
                    }
                }
                
                model.Buyer = user;
                model.CartItems = cartItems;
                return View("Checkout", model);
            }

            // Get farmer ID from first product
            var firstProduct = await _context.Products.FindAsync(cart.First().ProductId);
            if (firstProduct == null)
            {
                ModelState.AddModelError("", "Product not found");
                model.Buyer = user;
                model.CartItems = cart;
                return View("Checkout", model);
            }

            // Create order
            var orderCreatedAt = DateTime.UtcNow;
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                BuyerId = user.Id,
                FarmerId = firstProduct.FarmerId,
                TotalAmount = cart.Sum(item => item.TotalPrice),
                Status = OrderStatus.Pending,
                DeliveryAddress = model.DeliveryAddress,
                DeliveryCity = model.DeliveryCity,
                DeliveryProvince = model.DeliveryProvince,
                DeliveryPostalCode = model.DeliveryPostalCode,
                DeliveryNotes = model.DeliveryNotes,
                PaymentMethod = model.PaymentMethod,
                ResponseDeadline = orderCreatedAt.AddMinutes(30), // 30 minutes deadline
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
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.TotalPrice,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
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

            // Clear cart from database
            await ClearCartAsync(user.Id);

            // Get product names for notification
            var productNames = string.Join(", ", cart.Select(c => 
            {
                var product = _context.Products.Find(c.ProductId);
                return product?.Name ?? "Product";
            }).Take(3));
            
            if (cart.Count > 3)
            {
                productNames += $" and {cart.Count - 3} more";
            }

            // Create notification for farmer
            var deadlineTime = order.ResponseDeadline?.ToString("HH:mm") ?? "30 minutes";
            var notification = new Notification
            {
                UserId = order.FarmerId,
                Title = "New Order Received - Action Required",
                Message = $"You have received a new order #{order.OrderNumber} from {user.FirstName} {user.LastName} for {productNames}. Total: R{order.TotalAmount:N2}. Please accept or reject within 30 minutes (deadline: {deadlineTime} UTC).",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{order.OrderNumber} placed successfully! The farmer will review and respond within 30 minutes.";
            return RedirectToAction("OrdersHistory");
        }

        // Order Confirmation
        public async Task<IActionResult> OrderConfirmation(int id)
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
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            // Ensure OrderItems is initialized
            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }

            return View(order);
        }

        // Confirm Delivery - Show form with rating
        [HttpGet]
        public async Task<IActionResult> ConfirmDelivery(int id)
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
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Arrived)
            {
                TempData["Error"] = "This order is not ready for delivery confirmation.";
                return RedirectToAction("OrdersHistory");
            }

            // Check if already rated
            var existingRating = await _context.FarmerRatings
                .FirstOrDefaultAsync(r => r.OrderId == order.Id && r.BuyerId == user.Id);

            ViewBag.HasRated = existingRating != null;
            ViewBag.ExistingRating = existingRating;

            return View(order);
        }

        // Confirm Delivery - Process confirmation and rating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int id, int rating, string? comment)
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
                .Include(o => o.Farmer)
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Arrived)
            {
                TempData["Error"] = "This order is not ready for delivery confirmation.";
                return RedirectToAction("OrdersHistory");
            }

            // Validate rating
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please provide a valid rating (1-5 stars).";
                return RedirectToAction("ConfirmDelivery", new { id });
            }

            // Update order status
            order.Status = OrderStatus.Delivered;
            order.DeliveredDate = DateTime.UtcNow;
            order.IsDeliveryConfirmed = true;
            order.UpdatedAt = DateTime.UtcNow;

            // Save or update farmer rating
            var existingRating = await _context.FarmerRatings
                .FirstOrDefaultAsync(r => r.OrderId == order.Id && r.BuyerId == user.Id);

            if (existingRating != null)
            {
                existingRating.Rating = rating;
                existingRating.Comment = comment;
                existingRating.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var farmerRating = new FarmerRating
                {
                    FarmerId = order.FarmerId,
                    BuyerId = user.Id,
                    OrderId = order.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.FarmerRatings.Add(farmerRating);
            }

            // Create notification for farmer
            var notification = new Notification
            {
                UserId = order.FarmerId,
                Title = "Order Delivered",
                Message = $"Order #{order.OrderNumber} has been confirmed as delivered by the buyer. Rating: {rating}/5 stars.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Delivery confirmed successfully! Thank you for your rating.";
            return RedirectToAction("OrdersHistory");
        }

        // Orders History
        public async Task<IActionResult> OrdersHistory()
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

            var orders = await _context.Orders
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderTracking(int id)
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
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Add to Favorites
        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int productId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to add favorites" });
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Check if already favorited
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (existingFavorite != null)
            {
                return Json(new { success = false, message = "Already in favorites" });
            }

            // Add to favorites
            var favorite = new Favorite
            {
                UserId = user.Id,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Added to favorites" });
        }

        // Remove from Favorites
        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to manage favorites" });
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Removed from favorites" });
        }

        // Favorites / Wishlist
        public async Task<IActionResult> Favorites()
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

            var favorites = await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Farmer)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(favorites);
        }

        // Buyer Profile Settings
        public IActionResult ProfileSettings()
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

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
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

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return RedirectToAction("ProfileSettings");
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToAction("ProfileSettings");
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
                return RedirectToAction("ProfileSettings");
            }

            // Verify current password
            var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
            if (!authService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction("ProfileSettings");
            }

            // Update password
            user.PasswordHash = authService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("ProfileSettings");
        }

        // Messages / Chat
        public async Task<IActionResult> Messages()
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

            // Get conversations with farmers (based on orders)
            var conversations = await _context.Orders
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.BuyerId == user.Id)
                .GroupBy(o => o.FarmerId)
                .Select(g => new
                {
                    Farmer = g.First().Farmer,
                    LastOrder = g.OrderByDescending(o => o.CreatedAt).First(),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            return View(conversations);
        }

        public async Task<IActionResult> Chat(int farmerId)
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

            var farmer = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == farmerId && u.Role == UserRole.Farmer);

            if (farmer == null)
            {
                return NotFound();
            }

            // Check if buyer has any orders with this farmer
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.BuyerId == user.Id && o.FarmerId == farmerId);

            if (!hasOrders)
            {
                TempData["ErrorMessage"] = "You can only message farmers you have ordered from.";
                return RedirectToAction("Messages");
            }

            ViewBag.Farmer = farmer;
            return View();
        }

        // Reviews / Ratings
        public async Task<IActionResult> Reviews(int? productId)
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

            var reviews = await _context.ProductReviews
                .Include(r => r.Product)
                    .ThenInclude(p => p.Farmer)
                .Include(r => r.Buyer)
                .Where(r => r.BuyerId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            if (productId.HasValue)
            {
                var product = await _context.Products
                    .Include(p => p.Farmer)
                    .FirstOrDefaultAsync(p => p.Id == productId.Value);

                if (product == null)
                {
                    return NotFound();
                }

                ViewBag.Product = product;
            }

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int productId, int rating, string comment)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please log in to submit a review" });
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return Json(new { success = false, message = "Invalid user" });
            }

            // Check if user has purchased this product
            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId && oi.Order.BuyerId == user.Id && oi.Order.Status == OrderStatus.Delivered);

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "You can only review products you have purchased" });
            }

            // Check if user has already reviewed this product
            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.BuyerId == user.Id);

            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var review = new ProductReview
                {
                    ProductId = productId,
                    BuyerId = user.Id,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ProductReviews.Add(review);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Review submitted successfully" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
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

            var buyer = await _context.Users.FindAsync(user.Id);
            if (buyer == null)
            {
                return NotFound();
            }

            buyer.FirstName = model.FirstName;
            buyer.LastName = model.LastName;
            buyer.PhoneNumber = model.PhoneNumber;
            buyer.Address = model.Address;
            buyer.City = model.City;
            buyer.Province = model.Province;
            buyer.PostalCode = model.PostalCode;
            buyer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update session
            _sessionService.SetUser(buyer);

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("ProfileSettings");
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

        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
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

        private List<int> GetFavoritesFromSession()
        {
            var favoritesJson = HttpContext.Session.GetString("Favorites");
            return string.IsNullOrEmpty(favoritesJson) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(favoritesJson) ?? new List<int>();
        }

        private void SaveFavoritesToSession(List<int> favorites)
        {
            var favoritesJson = JsonSerializer.Serialize(favorites);
            HttpContext.Session.SetString("Favorites", favoritesJson);
        }

        private async Task<bool> IsProductFavorited(int productId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return false;
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Buyer)
            {
                return false;
            }

            return await _context.Favorites
                .AnyAsync(f => f.UserId == user.Id && f.ProductId == productId);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}
