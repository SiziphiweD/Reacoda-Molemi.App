using Microsoft.AspNetCore.Mvc;
using ReacodeApp.Models;
using ReacodeApp.Services;
using ReacodeApp.Data;

namespace ReacodeApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ISessionService sessionService, ApplicationDbContext context)
        {
            _authService = authService;
            _sessionService = sessionService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.LoginAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            _sessionService.SetUser(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect based on user role
            return user.Role switch
            {
                UserRole.Farmer => RedirectToAction("Dashboard", "Farmer"),
                UserRole.Buyer => RedirectToAction("Index", "Shop"),
                UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRole.SuperAdmin => RedirectToAction("Dashboard", "SuperAdmin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _authService.UserExistsAsync(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                return View(model);
            }

            var user = await _authService.RegisterAsync(model);
            _sessionService.SetUser(user);

            // Redirect based on user role
            return user.Role switch
            {
                UserRole.Farmer => RedirectToAction("Dashboard", "Farmer"),
                UserRole.Buyer => RedirectToAction("Index", "Shop"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _sessionService.ClearUser();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return View();
            }

            // In a real application, you would send a password reset email here
            // For now, we'll just show a success message
            TempData["Message"] = "Password reset link sent to your email address.";
            return RedirectToAction("Login");
        }
    }
}
