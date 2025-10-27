using Microsoft.AspNetCore.Mvc;
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

            return View(notifications);
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
