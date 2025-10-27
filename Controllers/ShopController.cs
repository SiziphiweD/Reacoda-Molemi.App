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
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
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

            return Json(new { success = true, message = "Added to cart" });
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

        public IActionResult Checkout()
        {
            if (!_sessionService.IsLoggedIn() || !_sessionService.IsBuyer())
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Cart");
            }

            return View(cart);
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
