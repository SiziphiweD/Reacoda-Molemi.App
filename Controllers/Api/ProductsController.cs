using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ReacodeApp.Data;
using ReacodeApp.Models;

namespace ReacodeApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all products (public endpoint)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsAvailable && p.Status == ProductStatus.Approved)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    PricePerKg = p.PricePerKg,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    Location = p.Location,
                    HarvestDate = p.HarvestDate,
                    ExpiryDate = p.ExpiryDate,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    FarmerId = p.FarmerId,
                    FarmerName = $"{p.Farmer.FirstName} {p.Farmer.LastName}",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// Get a specific product by ID (public endpoint)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    PricePerKg = p.PricePerKg,
                    AvailableQuantity = p.AvailableQuantity,
                    ImageUrl = p.ImageUrl,
                    Location = p.Location,
                    HarvestDate = p.HarvestDate,
                    ExpiryDate = p.ExpiryDate,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    FarmerId = p.FarmerId,
                    FarmerName = $"{p.Farmer.FirstName} {p.Farmer.LastName}",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product);
        }

        /// <summary>
        /// Create a new product (requires authentication)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
        {
            // Check if request body is null
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required", errors = new[] { "Request body cannot be null" } });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { field = x.Key, errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            // Get user ID from JWT token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Verify user exists and is a farmer
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Farmer)
            {
                return Forbid("Only farmers can create products");
            }

            // Verify category exists
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category not found" });
            }

            // Create product
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                PricePerKg = request.PricePerKg,
                AvailableQuantity = request.AvailableQuantity,
                ImageUrl = request.ImageUrl,
                Location = request.Location,
                HarvestDate = request.HarvestDate,
                ExpiryDate = request.ExpiryDate,
                CategoryId = request.CategoryId,
                FarmerId = userId,
                IsAvailable = true,
                Status = ProductStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load related data for response
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();
            await _context.Entry(product)
                .Reference(p => p.Farmer)
                .LoadAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PricePerKg = product.PricePerKg,
                AvailableQuantity = product.AvailableQuantity,
                ImageUrl = product.ImageUrl,
                Location = product.Location,
                HarvestDate = product.HarvestDate,
                ExpiryDate = product.ExpiryDate,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                FarmerId = product.FarmerId,
                FarmerName = $"{product.Farmer.FirstName} {product.Farmer.LastName}",
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }
    }

    // DTOs
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerKg { get; set; }
        public int AvailableQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }
        public DateTime? HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price per kg is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
        public decimal PricePerKg { get; set; }

        [Required(ErrorMessage = "Available quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int AvailableQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public string? Location { get; set; }

        public DateTime? HarvestDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }
    }
}

