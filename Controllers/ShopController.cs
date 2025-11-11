using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;

namespace ReacodeApp.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;

        public ShopController(ApplicationDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Index(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsAvailable && p.Status == ProductStatus.Approved);

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.PricePerKg >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.PricePerKg <= maxPrice.Value);
            }

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            var productList = await products.ToListAsync();

            var viewModel = new ShopIndexViewModel
            {
                Products = productList,
                Categories = categories,
                SearchTerm = search,
                SelectedCategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.Buyer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Allow guests to add to session cart, but require login for checkout
            // The view will redirect guests to registration anyway
            if (!_sessionService.IsLoggedIn() || !_sessionService.IsBuyer())
            {
                return Json(new { success = false, message = "Please login as a buyer" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsAvailable)
            {
                return Json(new { success = false, message = "Product not available" });
            }

            if (quantity > product.AvailableQuantity)
            {
                return Json(new { success = false, message = "Insufficient quantity available" });
            }

            // Add to session cart (you can implement a proper cart service later)
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            
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

            SetCart(cart);

            var cartCount = cart.Sum(item => item.Quantity);
            return Json(new { success = true, message = "Added to cart", cartCount = cartCount });
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                var cart = GetCart();
                var count = cart != null ? cart.Sum(item => item.Quantity) : 0;
                return Json(new { count = count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }

        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            SetCart(cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SetCart(cart);
            return RedirectToAction("Cart");
        }

        public IActionResult Checkout(int? productId = null, int? quantity = null)
        {
            if (!_sessionService.IsLoggedIn() || !_sessionService.IsBuyer())
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = GetCart();
            
            // If productId and quantity are provided, add to cart first
            if (productId.HasValue && quantity.HasValue)
            {
                var product = _context.Products.FindAsync(productId.Value).Result;
                if (product != null && product.IsAvailable)
                {
                    var existingItem = cart.FirstOrDefault(c => c.ProductId == productId.Value);
                    if (existingItem != null)
                    {
                        existingItem.Quantity = quantity.Value;
                    }
                    else
                    {
                        cart.Add(new CartItem
                        {
                            ProductId = productId.Value,
                            ProductName = product.Name,
                            Price = product.PricePerKg,
                            Quantity = quantity.Value,
                            ImageUrl = product.ImageUrl
                        });
                    }
                    SetCart(cart);
                }
            }

            if (!cart.Any())
            {
                return RedirectToAction("Cart");
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderRequest request)
        {
            if (!_sessionService.IsLoggedIn() || !_sessionService.IsBuyer())
            {
                return Json(new { success = false, message = "Please login to place an order" });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Cart is empty" });
            }

            try
            {
                // Get farmer ID from first product
                var firstProduct = await _context.Products.FindAsync(cart.First().ProductId);
                if (firstProduct == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

                // Create order
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    BuyerId = user.Id,
                    FarmerId = firstProduct.FarmerId,
                    TotalAmount = cart.Sum(item => item.TotalPrice),
                    Status = OrderStatus.Pending,
                    DeliveryAddress = request.DeliveryAddress,
                    DeliveryCity = request.City,
                    DeliveryProvince = request.City, // You may want to add province field
                    DeliveryPostalCode = request.PostalCode,
                    DeliveryNotes = request.DeliveryNotes,
                    PaymentMethod = Enum.Parse<PaymentMethod>(request.PaymentMethod),
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
                var notification = new Notification
                {
                    UserId = order.FarmerId,
                    Title = "New Order Received",
                    Message = $"You have received a new order #{order.OrderNumber} from {user.FirstName} {user.LastName} for {productNames}. Total: R{order.TotalAmount:N2}",
                    Type = NotificationType.Order,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Clear cart
                SetCart(new List<CartItem>());

                return Json(new { success = true, orderId = order.Id, message = "Order placed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error placing order: {ex.Message}" });
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();
            
            return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SetCart(List<CartItem> cart)
        {
            var cartJson = System.Text.Json.JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }

}
