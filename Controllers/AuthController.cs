using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Models;
using ReacodeApp.Services;
using ReacodeApp.Data;
using System.Text.Json;

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

            // Load cart from database for logged-in buyers
            if (user.Role == UserRole.Buyer)
            {
                await LoadCartToSessionAsync(user.Id);
            }

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
        public IActionResult Register(string? returnUrl = null, string? role = null)
        {
            if (_sessionService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.PreSelectedRole = role; // Pass role to view for pre-selection
            
            // Show message if user was redirected from add to cart or trying to buy
            if (!string.IsNullOrEmpty(returnUrl) && (returnUrl.Contains("/Shop/") || returnUrl.Contains("/Buyer/Shop") || returnUrl.Contains("/ProductDetails") || returnUrl.Contains("/Buyer/ProductDetails")))
            {
                TempData["InfoMessage"] = "Please create an account or sign in to add items to your cart and place orders.";
            }
            else if (!string.IsNullOrEmpty(returnUrl))
            {
                // Generic message for any return URL
                TempData["InfoMessage"] = "Please create an account or sign in to continue.";
            }
            
            // Create view model with pre-selected role if provided
            var model = new RegisterViewModel();
            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, ignoreCase: true, out var userRole))
            {
                model.SelectedRole = userRole;
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            if (await _authService.UserExistsAsync(model.Email))
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var user = await _authService.RegisterAsync(model);
            _sessionService.SetUser(user);

            // Load cart from database for logged-in buyers (will be empty for new users)
            if (user.Role == UserRole.Buyer)
            {
                await LoadCartToSessionAsync(user.Id);
            }

            // If returnUrl is provided and valid, redirect there
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

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
        public Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Task.FromResult<IActionResult>(View());
            }

            // In a real application, you would send a password reset email here
            // For now, we'll just show a success message
            TempData["Message"] = "Password reset link sent to your email address.";
            return Task.FromResult<IActionResult>(RedirectToAction("Login"));
        }

        // Helper method to load cart from database to session
        private async Task LoadCartToSessionAsync(int userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.IsActive)
                .ToListAsync();

            var cart = cartItems.Select(c => new CartItem
            {
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                Price = c.Product.PricePerKg,
                Quantity = c.Quantity,
                ImageUrl = c.Product.ImageUrl
            }).ToList();

            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}
