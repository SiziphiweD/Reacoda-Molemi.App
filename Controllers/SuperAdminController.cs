using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;
using System.Diagnostics;

namespace ReacodeApp.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionService _sessionService;
        private readonly IAuthService _authService;

        public SuperAdminController(ApplicationDbContext context, ISessionService sessionService, IAuthService authService)
        {
            _context = context;
            _sessionService = sessionService;
            _authService = authService;
        }

        private User? GetCurrentUser()
        {
            var userJson = HttpContext.Session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }
            return System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
        }

        private bool IsSuperAdmin()
        {
            var currentUser = GetCurrentUser();
            return currentUser != null && currentUser.Role == UserRole.SuperAdmin;
        }

        // GET: SuperAdmin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            var totalUsers = await _context.Users.CountAsync();
            var totalFarmers = await _context.Users.CountAsync(u => u.Role == UserRole.Farmer && u.IsActive);
            var totalBuyers = await _context.Users.CountAsync(u => u.Role == UserRole.Buyer && u.IsActive);
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive);
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount);
            var platformCommission = totalRevenue * 0.05m; // 5% platform commission

            var recentOrders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Farmer)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            var userGrowthData = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var revenueData = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= DateTime.UtcNow.AddMonths(-12))
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Revenue = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var viewModel = new SuperAdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalFarmers = totalFarmers,
                TotalBuyers = totalBuyers,
                TotalAdmins = totalAdmins,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                PlatformCommission = platformCommission,
                RecentOrders = recentOrders,
                RecentUsers = recentUsers,
                UserGrowthData = userGrowthData.Cast<dynamic>().ToList(),
                RevenueData = revenueData.Cast<dynamic>().ToList()
            };

            return View(viewModel);
        }

        // GET: SuperAdmin/AdminManagement
        public async Task<IActionResult> AdminManagement()
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(admins);
        }

        // POST: SuperAdmin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(string firstName, string lastName, string email, string password)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "All fields are required." });
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Email already exists." });
            }

            var admin = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = _authService.HashPassword(password),
                Role = UserRole.Admin,
                IsVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Admin created successfully." });
        }

        // POST: SuperAdmin/UpdateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdmin(int id, string firstName, string lastName, string email, bool isActive)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                return Json(new { success = false, message = "Admin not found." });
            }

            admin.FirstName = firstName;
            admin.LastName = lastName;
            admin.Email = email;
            admin.IsActive = isActive;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Admin updated successfully." });
        }

        // POST: SuperAdmin/DeleteAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                return Json(new { success = false, message = "Admin not found." });
            }

            _context.Users.Remove(admin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Admin deleted successfully." });
        }

        // GET: SuperAdmin/SystemConfiguration
        public IActionResult SystemConfiguration()
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            var config = new SystemConfigurationViewModel
            {
                PlatformCommissionRate = 5.0m,
                PaymentGatewayEnabled = true,
                EmailNotificationsEnabled = true,
                PlatformName = "Reacoda Molemi",
                SupportEmail = "support@molemi.com",
                MaxProductImages = 5,
                OrderTimeoutHours = 24
            };

            return View(config);
        }

        // POST: SuperAdmin/UpdateSystemConfiguration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSystemConfiguration(SystemConfigurationViewModel model)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            // In a real application, you would save these settings to a database
            // For now, we'll just return success
            return Json(new { success = true, message = "System configuration updated successfully." });
        }

        // GET: SuperAdmin/AuditLogs
        public async Task<IActionResult> AuditLogs(DateTime? startDate, DateTime? endDate, string userId = "all")
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var query = _context.Notifications.AsQueryable();

            if (userId != "all" && int.TryParse(userId, out int userIdInt))
            {
                query = query.Where(n => n.UserId == userIdInt);
            }

            var auditLogs = await query
                .Include(n => n.User)
                .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.SelectedUserId = userId;
            ViewBag.Users = await _context.Users.ToListAsync();

            return View(auditLogs);
        }

        // GET: SuperAdmin/GlobalAnalytics
        public async Task<IActionResult> GlobalAnalytics(DateTime? startDate, DateTime? endDate)
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            startDate ??= DateTime.UtcNow.AddMonths(-6);
            endDate ??= DateTime.UtcNow;

            var analytics = new GlobalAnalyticsViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                TotalRevenue = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount),
                TotalOrders = await _context.Orders.CountAsync(),
                AverageOrderValue = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).AnyAsync() 
                    ? await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).AverageAsync(o => o.TotalAmount)
                    : 0,
                TopProducts = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new TopProductAnalytics
                    {
                        ProductName = g.First().Product.Name,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.TotalPrice)
                    })
                    .OrderByDescending(tp => tp.Revenue)
                    .Take(10)
                    .ToListAsync(),
                UserGrowthByMonth = await _context.Users
                    .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                    .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                    .Select(g => new UserGrowthData
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        NewUsers = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync(),
                RevenueByMonth = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new RevenueData
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync()
            };

            return View(analytics);
        }

        // GET: SuperAdmin/RevenueManagement
        public async Task<IActionResult> RevenueManagement(DateTime? startDate, DateTime? endDate)
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            startDate ??= DateTime.UtcNow.AddMonths(-3);
            endDate ??= DateTime.UtcNow;

            var revenue = new RevenueManagementViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalRevenue = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount),
                PlatformCommission = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount) * 0.05m,
                FarmerEarnings = await _context.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => o.TotalAmount) * 0.95m,
                TotalTransactions = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered),
                CommissionRate = 5.0m,
                RevenueByMonth = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Delivered && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new RevenueByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(o => o.TotalAmount),
                        Commission = g.Sum(o => o.TotalAmount) * 0.05m
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync()
            };

            return View(revenue);
        }

        // GET: SuperAdmin/PlatformSettings
        public IActionResult PlatformSettings()
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            var settings = new PlatformSettingsViewModel
            {
                PlatformName = "Reacoda Molemi",
                PlatformDescription = "Connecting farmers and buyers for fresh produce",
                PrimaryColor = "#1FAA59",
                SecondaryColor = "#E74C3C",
                LogoUrl = "/images/logo.png",
                FaviconUrl = "/images/favicon.ico",
                ContactEmail = "contact@molemi.com",
                ContactPhone = "+27 11 123 4567",
                Address = "123 Agriculture Street, Johannesburg, South Africa",
                SocialMediaFacebook = "https://facebook.com/molemi",
                SocialMediaTwitter = "https://twitter.com/molemi",
                SocialMediaInstagram = "https://instagram.com/molemi"
            };

            return View(settings);
        }

        // POST: SuperAdmin/UpdatePlatformSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePlatformSettings(PlatformSettingsViewModel model)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            // In a real application, you would save these settings to a database
            return Json(new { success = true, message = "Platform settings updated successfully." });
        }

        // GET: SuperAdmin/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsSuperAdmin()) return RedirectToAction("Login", "Auth");

            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Auth");

            var superAdmin = await _context.Users.FindAsync(currentUser.Id);
            if (superAdmin == null) return RedirectToAction("Login", "Auth");

            return View(superAdmin);
        }

        // POST: SuperAdmin/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Json(new { success = false, message = "Not logged in" });

            var userToUpdate = await _context.Users.FindAsync(currentUser.Id);
            if (userToUpdate == null) return Json(new { success = false, message = "User not found" });

            userToUpdate.FirstName = model.FirstName;
            userToUpdate.LastName = model.LastName;
            userToUpdate.Email = model.Email;
            userToUpdate.PhoneNumber = model.PhoneNumber;
            userToUpdate.Address = model.Address;
            userToUpdate.City = model.City;
            userToUpdate.Province = model.Province;
            userToUpdate.PostalCode = model.PostalCode;
            userToUpdate.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(userToUpdate);
            await _context.SaveChangesAsync();

            // Update session with new user data
            _sessionService.SetUser(userToUpdate);

            return Json(new { success = true, message = "Profile updated successfully." });
        }

        // POST: SuperAdmin/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!IsSuperAdmin()) return Json(new { success = false, message = "Unauthorized" });

            var currentUser = GetCurrentUser();
            if (currentUser == null) return Json(new { success = false, message = "Not logged in" });

            var user = await _context.Users.FindAsync(currentUser.Id);
            if (user == null) return Json(new { success = false, message = "User not found" });

            if (!_authService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                return Json(new { success = false, message = "Incorrect current password." });
            }

            user.PasswordHash = _authService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password changed successfully." });
        }
    }
}