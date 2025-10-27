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

            var viewModel = new BuyerDashboardViewModel
            {
                Buyer = user,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                DeliveredOrders = deliveredOrders,
                TotalSpent = totalSpent,
                RecentOrders = recentOrders,
                FavoriteProducts = favoriteProducts,
                SuggestedProducts = suggestedProducts
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

            // Get cart from session
            var cart = GetCartFromSession();
            
            // Check if item already exists in cart
            var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Price = product.PricePerKg,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            // Save cart to session
            SaveCartToSession(cart);

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Cart saved with {cart.Count} items, total quantity: {cart.Sum(item => item.Quantity)}");

            return Json(new { success = true, message = "Added to cart", cartCount = cart.Sum(item => item.Quantity) });
        }

        // Get Cart Count
        [HttpGet]
        public IActionResult GetCartCount()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, count = 0 });
            }

            var cart = GetCartFromSession();
            var count = cart.Sum(item => item.Quantity);
            
            return Json(new { success = true, count = count });
        }

        // Cart Page
        public IActionResult Cart()
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

            var cart = GetCartFromSession();
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Cart retrieved with {cart.Count} items");
            foreach (var item in cart)
            {
                System.Diagnostics.Debug.WriteLine($"Item: {item.ProductName}, Quantity: {item.Quantity}, Price: {item.Price}");
            }
            
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

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found in cart" });
            }

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                // Check product availability
                var product = await _context.Products.FindAsync(productId);
                if (product == null || quantity > product.AvailableQuantity)
                {
                    return Json(new { success = false, message = "Invalid quantity" });
                }
                
                item.Quantity = quantity;
            }

            SaveCartToSession(cart);
            return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity), total = cart.Sum(item => item.TotalPrice) });
        }

        // Remove from Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false, message = "Please login" });
            }

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return Json(new { success = true, cartCount = cart.Sum(item => item.Quantity), total = cart.Sum(item => item.TotalPrice) });
        }

        // Checkout Page
        public IActionResult Checkout()
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

            var cart = GetCartFromSession();
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
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
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

            var cart = GetCartFromSession();
            if (!cart.Any())
            {
                return RedirectToAction("Cart");
            }

            if (!ModelState.IsValid)
            {
                model.Buyer = user;
                model.CartItems = cart;
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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

            // Clear cart
            ClearCart();

            // Create notification for farmer
            var notification = new Notification
            {
                UserId = order.FarmerId,
                Title = "New Order Received",
                Message = $"You have received a new order #{order.Id} from {user.FirstName} {user.LastName}.",
                Type = NotificationType.Order,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
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
                .FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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

        private void ClearCart()
        {
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
