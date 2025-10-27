using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;

namespace ReacodeApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;
        private readonly IAuthService _authService;

        public AdminController(ApplicationDbContext context, ISessionService sessionService, IAuthService authService)
        {
            _context = context;
            _sessionService = sessionService;
            _authService = authService;
        }

        // Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var adminDashboard = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
                TotalFarmers = await _context.Users.CountAsync(u => u.Role == UserRole.Farmer && u.IsActive),
                TotalBuyers = await _context.Users.CountAsync(u => u.Role == UserRole.Buyer && u.IsActive),
                PendingApprovals = await _context.Users.CountAsync(u => !u.IsVerified && u.IsActive),
                TotalProducts = await _context.Products.CountAsync(p => p.IsAvailable),
                PendingProductApprovals = await _context.Products.CountAsync(p => p.Status == ProductStatus.Pending),
                TotalOrders = await _context.Orders.CountAsync(o => o.IsActive),
                TotalSales = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount),
                RecentOrders = await _context.Orders
                    .Include(o => o.Buyer)
                    .Include(o => o.Farmer)
                    .Where(o => o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentUsers = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(adminDashboard);
        }

        // User Management
        public async Task<IActionResult> UserManagement(string role = "all", string status = "all")
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Users.AsQueryable();

            if (role != "all")
            {
                if (Enum.TryParse<UserRole>(role, true, out var userRole))
                {
                    query = query.Where(u => u.Role == userRole);
                }
            }

            if (status != "all")
            {
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(u => u.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(u => !u.IsActive);
                        break;
                    case "verified":
                        query = query.Where(u => u.IsVerified);
                        break;
                    case "unverified":
                        query = query.Where(u => !u.IsVerified);
                        break;
                }
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            ViewBag.Role = role;
            ViewBag.Status = status;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "User approved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> SuspendUser(int userId)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "User suspended successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "User deleted successfully" });
        }

        // Product Verification
        public async Task<IActionResult> ProductVerification()
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var pendingProducts = await _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.Status == ProductStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(pendingProducts);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProduct(int productId)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            product.Status = ProductStatus.Approved;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product approved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectProduct(int productId, string reason = "")
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            product.Status = ProductStatus.Rejected;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product rejected successfully" });
        }

        // Orders Management
        public async Task<IActionResult> OrdersManagement(string status = "all")
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var query = _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Farmer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (status != "all")
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            ViewBag.Status = status;

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == OrderStatus.Shipped)
            {
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (status == OrderStatus.Delivered)
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order status updated successfully" });
        }

        // Reports & Analytics
        public async Task<IActionResult> Reports(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var reports = new AdminReportsViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalSales = await _context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.TotalAmount),
                TotalOrders = await _context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .CountAsync(),
                NewUsers = await _context.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .CountAsync(),
                NewProducts = await _context.Products
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                    .CountAsync(),
                SalesByMonth = await _context.Orders
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Delivered)
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new SalesByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Sales = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(s => s.Year)
                    .ThenBy(s => s.Month)
                    .ToListAsync(),
                TopProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopProduct
                    {
                        ProductName = g.First().Product.Name,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(tp => tp.QuantitySold)
                    .Take(10)
                    .ToListAsync()
            };

            return View(reports);
        }

        // Messages / Support Center
        public async Task<IActionResult> Messages()
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var messages = await _context.Notifications
                .Include(n => n.User)
                .Where(n => n.Type == NotificationType.System)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int userId, string title, string message)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.System,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Message sent successfully" });
        }

        // Admin Profile Settings
        public IActionResult ProfileSettings()
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.Province = model.Province;
            user.PostalCode = model.PostalCode;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Profile updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var user = await _context.Users.FindAsync(_sessionService.GetUser().Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Verify current password
            if (!_authService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            user.PasswordHash = _authService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password changed successfully" });
        }

        // Announcements / Notifications
        public async Task<IActionResult> Announcements()
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return RedirectToAction("Login", "Auth");
            }

            var announcements = await _context.Notifications
                .Where(n => n.Type == NotificationType.System)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> SendAnnouncement(string title, string message, string targetRole = "all")
        {
            if (!_sessionService.IsLoggedIn() || (_sessionService.GetUser()?.Role != UserRole.Admin && _sessionService.GetUser()?.Role != UserRole.SuperAdmin))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var users = _context.Users.Where(u => u.IsActive).AsQueryable();

            if (targetRole != "all")
            {
                if (Enum.TryParse<UserRole>(targetRole, true, out var role))
                {
                    users = users.Where(u => u.Role == role);
                }
            }

            var userList = await users.ToListAsync();

            foreach (var user in userList)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    Type = NotificationType.System,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Announcement sent to {userList.Count} users" });
        }
    }
}