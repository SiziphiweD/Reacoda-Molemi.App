using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;

namespace ReacodeApp.Controllers
{
    public class FarmerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;

        public FarmerController(ApplicationDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // Farmer Dashboard - Overview of products, orders, earnings
        public async Task<IActionResult> Dashboard()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var farmerId = user.Id;

            // Get farmer statistics
            var totalProducts = await _context.Products.CountAsync(p => p.FarmerId == farmerId);
            var activeProducts = await _context.Products.CountAsync(p => p.FarmerId == farmerId && p.IsAvailable);
            var totalOrders = await _context.Orders.CountAsync(o => o.FarmerId == farmerId);
            var pendingOrders = await _context.Orders.CountAsync(o => o.FarmerId == farmerId && o.Status == OrderStatus.Pending);
            var totalEarnings = await _context.Orders
                .Where(o => o.FarmerId == farmerId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Buyer)
                .Where(o => o.FarmerId == farmerId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get recent notifications
            var notifications = await _context.Notifications
                .Where(n => n.UserId == farmerId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new FarmerDashboardViewModel
            {
                Farmer = user,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalEarnings = totalEarnings,
                RecentOrders = recentOrders,
                Notifications = notifications
            };

            return View(viewModel);
        }

        // My Products - List all farmer's products
        public async Task<IActionResult> MyProducts()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.FarmerId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // Add Product - Form to add new product
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var viewModel = new AddProductViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                PricePerKg = model.PricePerKg,
                AvailableQuantity = model.AvailableQuantity,
                CategoryId = model.CategoryId,
                FarmerId = user.Id,
                Location = model.Location,
                HarvestDate = model.HarvestDate,
                ExpiryDate = model.ExpiryDate,
                ImageUrl = model.ImageUrl ?? "/images/default-product.jpg",
                IsAvailable = true,
                IsActive = true,
                Status = ProductStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Product added successfully!";
            return RedirectToAction("MyProducts");
        }

        // Edit Product - Update existing product
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var viewModel = new EditProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PricePerKg = product.PricePerKg,
                AvailableQuantity = product.AvailableQuantity,
                CategoryId = product.CategoryId,
                Location = product.Location,
                HarvestDate = product.HarvestDate,
                ExpiryDate = product.ExpiryDate,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable,
                Categories = categories
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(EditProductViewModel model)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.FarmerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.PricePerKg = model.PricePerKg;
            product.AvailableQuantity = model.AvailableQuantity;
            product.CategoryId = model.CategoryId;
            product.Location = model.Location;
            product.HarvestDate = model.HarvestDate;
            product.ExpiryDate = model.ExpiryDate;
            product.ImageUrl = model.ImageUrl ?? product.ImageUrl;
            product.IsAvailable = model.IsAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Product updated successfully!";
            return RedirectToAction("MyProducts");
        }

        // Orders Received - Show buyer orders
        public async Task<IActionResult> OrdersReceived()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.FarmerId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // Order Details - View full order info
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Accept Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Accepted;
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Confirmed",
                Message = $"Your order #{order.Id} has been confirmed by the farmer.",
                Type = NotificationType.Order,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order accepted successfully!";
            return RedirectToAction("OrderDetails", new { id });
        }

        // Reject Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrder(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Rejected",
                Message = $"Your order #{order.Id} has been rejected by the farmer.",
                Type = NotificationType.Order,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order rejected.";
            return RedirectToAction("OrderDetails", new { id });
        }

        // Mark as Shipped
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = OrderStatus.Shipped;
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Shipped",
                Message = $"Your order #{order.Id} has been shipped and is on its way.",
                Type = NotificationType.Order,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order marked as shipped!";
            return RedirectToAction("OrderDetails", new { id });
        }

        // Earnings/Payouts - Summary of sales
        public async Task<IActionResult> Earnings()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var farmerId = user.Id;

            // Get earnings summary
            var totalEarnings = await _context.Orders
                .Where(o => o.FarmerId == farmerId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var pendingEarnings = await _context.Orders
                .Where(o => o.FarmerId == farmerId && (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Shipped))
                .SumAsync(o => o.TotalAmount);

            var monthlyEarnings = await _context.Orders
                .Where(o => o.FarmerId == farmerId && 
                           o.Status == OrderStatus.Delivered && 
                           o.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                .SumAsync(o => o.TotalAmount);

            // Get recent transactions
            var recentTransactions = await _context.Orders
                .Include(o => o.Buyer)
                .Where(o => o.FarmerId == farmerId && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var viewModel = new FarmerEarningsViewModel
            {
                Farmer = user,
                TotalEarnings = totalEarnings,
                PendingEarnings = pendingEarnings,
                MonthlyEarnings = monthlyEarnings,
                RecentTransactions = recentTransactions
            };

            return View(viewModel);
        }

        // Messages/Chat - Communicate with buyers
        public async Task<IActionResult> Messages()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            // Get buyers who have ordered from this farmer
            var buyers = await _context.Orders
                .Include(o => o.Buyer)
                .Where(o => o.FarmerId == user.Id)
                .Select(o => o.Buyer)
                .Distinct()
                .ToListAsync();

            return View(buyers);
        }

        // Farmer Profile Settings
        public async Task<IActionResult> ProfileSettings()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(user);
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
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var farmer = await _context.Users.FindAsync(user.Id);
            if (farmer == null)
            {
                return NotFound();
            }

            farmer.FirstName = model.FirstName;
            farmer.LastName = model.LastName;
            farmer.PhoneNumber = model.PhoneNumber;
            farmer.Address = model.Address;
            farmer.City = model.City;
            farmer.Province = model.Province;
            farmer.PostalCode = model.PostalCode;
            farmer.BankName = model.BankName;
            farmer.BankAccountNumber = model.BankAccountNumber;
            farmer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update session
            _sessionService.SetUser(farmer);

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("ProfileSettings");
        }

        // Reviews Page - View buyer reviews
        public async Task<IActionResult> Reviews()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var reviews = await _context.ProductReviews
                .Include(r => r.Product)
                .Include(r => r.Buyer)
                .Where(r => r.Product.FarmerId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // Toggle Product Availability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null || user.Role != UserRole.Farmer)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == user.Id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsAvailable = !product.IsAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Product {(product.IsAvailable ? "marked as available" : "marked as unavailable")}.";
            return RedirectToAction("MyProducts");
        }
    }
}