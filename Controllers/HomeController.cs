using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Models;
using ReacodeApp.Services;
using ReacodeApp.Data;

namespace ReacodeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISessionService _sessionService;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ISessionService sessionService, ApplicationDbContext context)
        {
            _logger = logger;
            _sessionService = sessionService;
            _context = context;
        }

        public IActionResult Index()
        {
            // If user is logged in, redirect to appropriate dashboard
            if (_sessionService.IsLoggedIn())
            {
                var user = _sessionService.GetUser();
                return user?.Role switch
                {
                    UserRole.Farmer => RedirectToAction("Dashboard", "Farmer"),
                    UserRole.Buyer => RedirectToAction("Index", "Shop"),
                    UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                    UserRole.SuperAdmin => RedirectToAction("Dashboard", "SuperAdmin"),
                    _ => View()
                };
            }

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Profile()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            return View(user);
        }

        public IActionResult Notifications()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _sessionService.GetUser();
            if (user == null) return RedirectToAction("Login", "Auth");
            
            var notifications = _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            // Mark notifications as viewed when user visits the page
            var lastViewTimeKey = $"LastNotificationView_{user.Id}";
            HttpContext.Session.SetString(lastViewTimeKey, DateTime.UtcNow.ToString("O"));

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { count = 0 });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            // Get the last time user viewed notifications from session
            var lastViewTimeKey = $"LastNotificationView_{user.Id}";
            var lastViewTimeStr = HttpContext.Session.GetString(lastViewTimeKey);
            DateTime? lastViewTime = null;
            
            if (!string.IsNullOrEmpty(lastViewTimeStr) && DateTime.TryParse(lastViewTimeStr, out var parsedTime))
            {
                lastViewTime = parsedTime;
            }

            // Count new notifications (created after last view time, or in last 24 hours if no view time)
            var cutoffTime = lastViewTime ?? DateTime.UtcNow.AddHours(-24);
            
            var newNotificationCount = await _context.Notifications
                .CountAsync(n => n.UserId == user.Id && n.CreatedAt > cutoffTime);

            return Json(new { count = newNotificationCount });
        }

        [HttpPost]
        public IActionResult MarkNotificationsAsViewed()
        {
            if (!_sessionService.IsLoggedIn())
            {
                return Json(new { success = false });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Json(new { success = false });
            }

            // Store the current time as the last view time in session
            var lastViewTimeKey = $"LastNotificationView_{user.Id}";
            HttpContext.Session.SetString(lastViewTimeKey, DateTime.UtcNow.ToString("O"));

            return Json(new { success = true });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
