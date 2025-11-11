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

            string? imageUrl = "/images/default-product.jpg";
            bool hasImage = false;

            // Handle image file upload
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFile", "Only JPG, PNG, and GIF images are allowed.");
                    model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    return View(model);
                }

                // Validate file size (5MB max)
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Image size must be less than 5MB.");
                    model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                    return View(model);
                }

                // Create images/products directory if it doesn't exist
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(imagesPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/images/products/{fileName}";
                hasImage = true;
            }

            // Determine product status: Pending if image uploaded, otherwise Approved
            var productStatus = hasImage ? ProductStatus.Pending : ProductStatus.Approved;

            // Convert dates to UTC for PostgreSQL
            DateTime? harvestDateUtc = null;
            if (model.HarvestDate.HasValue)
            {
                harvestDateUtc = model.HarvestDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(model.HarvestDate.Value, DateTimeKind.Utc)
                    : model.HarvestDate.Value.ToUniversalTime();
            }

            DateTime? expiryDateUtc = null;
            if (model.ExpiryDate.HasValue)
            {
                expiryDateUtc = model.ExpiryDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(model.ExpiryDate.Value, DateTimeKind.Utc)
                    : model.ExpiryDate.Value.ToUniversalTime();
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
                HarvestDate = harvestDateUtc,
                ExpiryDate = expiryDateUtc,
                ImageUrl = imageUrl,
                IsAvailable = productStatus == ProductStatus.Approved,
                IsActive = true,
                Status = productStatus,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Create notification for admin if product has image (needs approval)
            if (hasImage)
            {
                var admins = await _context.Users
                    .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    var notification = new Notification
                    {
                        UserId = admin.Id,
                        Title = "New Product Pending Approval",
                        Message = $"A new product '{product.Name}' has been submitted by {user.FirstName} {user.LastName} and requires approval.",
                        Type = NotificationType.Product,
                        IsRead = false,
                        RelatedEntityId = product.Id,
                        RelatedEntityType = "Product",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();

                TempData["Message"] = "Product added successfully! It is pending admin approval due to image upload.";
            }
            else
            {
                TempData["Message"] = "Product added successfully!";
            }

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

            // Convert dates to UTC for PostgreSQL
            DateTime? harvestDateUtc = null;
            if (model.HarvestDate.HasValue)
            {
                harvestDateUtc = model.HarvestDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(model.HarvestDate.Value, DateTimeKind.Utc)
                    : model.HarvestDate.Value.ToUniversalTime();
            }

            DateTime? expiryDateUtc = null;
            if (model.ExpiryDate.HasValue)
            {
                expiryDateUtc = model.ExpiryDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(model.ExpiryDate.Value, DateTimeKind.Utc)
                    : model.ExpiryDate.Value.ToUniversalTime();
            }

            product.Name = model.Name;
            product.Description = model.Description;
            product.PricePerKg = model.PricePerKg;
            product.AvailableQuantity = model.AvailableQuantity;
            product.CategoryId = model.CategoryId;
            product.Location = model.Location;
            product.HarvestDate = harvestDateUtc;
            product.ExpiryDate = expiryDateUtc;
            product.ImageUrl = model.ImageUrl ?? product.ImageUrl;
            product.IsAvailable = model.IsAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Product updated successfully!";
            return RedirectToAction("MyProducts");
        }

        // Orders Received - Show buyer orders
        public async Task<IActionResult> OrdersReceived(int? orderId = null)
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

            if (orderId.HasValue)
            {
                ViewBag.HighlightOrderId = orderId.Value;
            }

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
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

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

            // Check if order is still pending
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "This order has already been processed.";
                return RedirectToAction("OrdersReceived");
            }

            // Check if 30 minutes have passed
            if (order.ResponseDeadline.HasValue && DateTime.UtcNow > order.ResponseDeadline.Value)
            {
                TempData["Error"] = "The 30-minute deadline for accepting this order has passed. Please contact support.";
                return RedirectToAction("OrdersReceived");
            }

            order.Status = OrderStatus.Accepted;
            order.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3); // 3 days estimated delivery
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Accepted",
                Message = $"Your order #{order.OrderNumber} has been accepted by the farmer. Estimated delivery: {order.EstimatedDeliveryDate.Value:MMMM dd, yyyy}.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order accepted successfully!";
            return RedirectToAction("OrderDetails", new { id });
        }

        // Reject Order - Show form
        [HttpGet]
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
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "This order has already been processed.";
                return RedirectToAction("OrdersReceived");
            }

            return View(order);
        }

        // Reject Order - Process rejection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrder(int id, string rejectionReason)
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

            // Check if order is still pending
            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "This order has already been processed.";
                return RedirectToAction("OrdersReceived");
            }

            // Check if 30 minutes have passed
            if (order.ResponseDeadline.HasValue && DateTime.UtcNow > order.ResponseDeadline.Value)
            {
                TempData["Error"] = "The 30-minute deadline for rejecting this order has passed. Please contact support.";
                return RedirectToAction("OrdersReceived");
            }

            // Validate rejection reason
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["Error"] = "Please provide a reason for rejecting this order.";
                return RedirectToAction("RejectOrder", new { id });
            }

            order.Status = OrderStatus.Rejected;
            order.RejectionReason = rejectionReason;
            order.UpdatedAt = DateTime.UtcNow;

            // Restore product quantities
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                if (item.Product != null)
                {
                    item.Product.AvailableQuantity += item.Quantity;
                    if (item.Product.AvailableQuantity > 0 && !item.Product.IsAvailable)
                    {
                        item.Product.IsAvailable = true;
                    }
                }
            }

            // Create notification for buyer with rejection reason
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Rejected",
                Message = $"Your order #{order.OrderNumber} has been rejected by the farmer. Reason: {rejectionReason}",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order rejected successfully.";
            return RedirectToAction("OrdersReceived");
        }

        // Mark as Shipped - Show form
        [HttpGet]
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
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Accepted)
            {
                TempData["Error"] = "Only accepted orders can be marked as shipped.";
                return RedirectToAction("OrdersReceived");
            }

            return View(order);
        }

        // Mark as Shipped - Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(int id, string trackingNumber, DateTime? estimatedDeliveryDate)
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

            if (order.Status != OrderStatus.Accepted)
            {
                TempData["Error"] = "Only accepted orders can be marked as shipped.";
                return RedirectToAction("OrdersReceived");
            }

            order.Status = OrderStatus.Shipped;
            order.ShippedDate = DateTime.UtcNow;
            order.TrackingNumber = trackingNumber;
            if (estimatedDeliveryDate.HasValue)
            {
                order.EstimatedDeliveryDate = estimatedDeliveryDate.Value.ToUniversalTime();
            }
            else
            {
                order.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3); // Default 3 days
            }
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var trackingInfo = !string.IsNullOrWhiteSpace(trackingNumber) 
                ? $" Tracking Number: {trackingNumber}." 
                : "";
            var deliveryInfo = order.EstimatedDeliveryDate.HasValue 
                ? $" Estimated delivery: {order.EstimatedDeliveryDate.Value:MMMM dd, yyyy}." 
                : "";
            
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Shipped",
                Message = $"Your order #{order.OrderNumber} has been shipped and is on its way.{trackingInfo}{deliveryInfo}",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order marked as shipped successfully!";
            return RedirectToAction("OrdersReceived");
        }

        // Mark as Arrived - Farmer arrives at delivery location
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsArrived(int id)
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
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != OrderStatus.Shipped)
            {
                TempData["Error"] = "Only shipped orders can be marked as arrived.";
                return RedirectToAction("OrdersReceived");
            }

            order.Status = OrderStatus.Arrived;
            order.ArrivedDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Create notification for buyer
            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Farmer Has Arrived",
                Message = $"The farmer has arrived at your delivery location for order #{order.OrderNumber}. Please confirm that you have received your order.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order marked as arrived! Buyer has been notified.";
            return RedirectToAction("OrdersReceived");
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
        public IActionResult ProfileSettings()
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