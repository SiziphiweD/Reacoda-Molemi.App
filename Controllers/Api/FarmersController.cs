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
    [Authorize]
    public class FarmersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FarmersController> _logger;

        public FarmersController(ApplicationDbContext context, ILogger<FarmersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        private async Task<bool> IsFarmer(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.Role == UserRole.Farmer;
        }

        /// <summary>
        /// Get farmer dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<FarmerDashboardDto>> GetDashboard()
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var totalProducts = await _context.Products.CountAsync(p => p.FarmerId == userId);
            var activeProducts = await _context.Products.CountAsync(p => p.FarmerId == userId && p.IsAvailable);
            var totalOrders = await _context.Orders.CountAsync(o => o.FarmerId == userId);
            var pendingOrders = await _context.Orders.CountAsync(o => o.FarmerId == userId && o.Status == OrderStatus.Pending);
            var totalEarnings = await _context.Orders
                .Where(o => o.FarmerId == userId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentOrders = await _context.Orders
                .Include(o => o.Buyer)
                .Where(o => o.FarmerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    BuyerName = $"{o.Buyer.FirstName} {o.Buyer.LastName}",
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(new FarmerDashboardDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalEarnings = totalEarnings,
                RecentOrders = recentOrders
            });
        }

        /// <summary>
        /// Get all products for the logged-in farmer
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetMyProducts()
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.FarmerId == userId)
                .OrderByDescending(p => p.CreatedAt)
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
                    FarmerName = "",
                    Status = p.Status.ToString(),
                    IsAvailable = p.IsAvailable,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt ?? p.CreatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// Get a specific product by ID (farmer's own product)
        /// </summary>
        [HttpGet("products/{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.FarmerId == userId)
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
                    FarmerName = "",
                    Status = p.Status.ToString(),
                    IsAvailable = p.IsAvailable,
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
        /// Create a new product
        /// </summary>
        [HttpPost("products")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can create products");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { field = x.Key, errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                return BadRequest(new { message = "Validation failed", errors = errors });
            }

            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category not found" });
            }

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

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

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
                FarmerName = "",
                Status = product.Status.ToString(),
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        /// <summary>
        /// Update a product
        /// </summary>
        [HttpPut("products/{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can update products");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == userId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.PricePerKg = request.PricePerKg;
            product.AvailableQuantity = request.AvailableQuantity;
            product.ImageUrl = request.ImageUrl;
            product.Location = request.Location;
            product.HarvestDate = request.HarvestDate;
            product.ExpiryDate = request.ExpiryDate;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _context.Entry(product).Reference(p => p.Category).LoadAsync();

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
                FarmerName = "",
                Status = product.Status.ToString(),
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt
            };

            return Ok(productDto);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can delete products");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == userId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsActive = false;
            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        /// <summary>
        /// Toggle product availability
        /// </summary>
        [HttpPost("products/{id}/toggle-availability")]
        public async Task<ActionResult<ProductDto>> ToggleAvailability(int id)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can toggle product availability");
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.FarmerId == userId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            product.IsAvailable = !product.IsAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
                FarmerName = "",
                Status = product.Status.ToString(),
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt ?? product.CreatedAt
            };

            return Ok(productDto);
        }

        /// <summary>
        /// Get all orders received by the farmer
        /// </summary>
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersReceived()
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.FarmerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryCity = o.DeliveryCity,
                    DeliveryProvince = o.DeliveryProvince,
                    DeliveryPostalCode = o.DeliveryPostalCode,
                    DeliveryNotes = o.DeliveryNotes,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    TrackingNumber = o.TrackingNumber,
                    ShippedDate = o.ShippedDate,
                    ArrivedDate = o.ArrivedDate,
                    DeliveredDate = o.DeliveredDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    ResponseDeadline = o.ResponseDeadline,
                    RejectionReason = o.RejectionReason,
                    BuyerId = o.BuyerId,
                    BuyerName = $"{o.Buyer.FirstName} {o.Buyer.LastName}",
                    BuyerEmail = o.Buyer.Email,
                    BuyerPhone = o.Buyer.PhoneNumber,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt ?? o.CreatedAt
                })
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Get a specific order by ID
        /// </summary>
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id && o.FarmerId == userId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    DeliveryAddress = o.DeliveryAddress,
                    DeliveryCity = o.DeliveryCity,
                    DeliveryProvince = o.DeliveryProvince,
                    DeliveryPostalCode = o.DeliveryPostalCode,
                    DeliveryNotes = o.DeliveryNotes,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    TrackingNumber = o.TrackingNumber,
                    ShippedDate = o.ShippedDate,
                    ArrivedDate = o.ArrivedDate,
                    DeliveredDate = o.DeliveredDate,
                    EstimatedDeliveryDate = o.EstimatedDeliveryDate,
                    ResponseDeadline = o.ResponseDeadline,
                    RejectionReason = o.RejectionReason,
                    BuyerId = o.BuyerId,
                    BuyerName = $"{o.Buyer.FirstName} {o.Buyer.LastName}",
                    BuyerEmail = o.Buyer.Email,
                    BuyerPhone = o.Buyer.PhoneNumber,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt ?? o.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(order);
        }

        /// <summary>
        /// Accept an order
        /// </summary>
        [HttpPost("orders/{id}/accept")]
        public async Task<IActionResult> AcceptOrder(int id, [FromBody] AcceptOrderRequest? request = null)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can accept orders");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status != OrderStatus.Pending)
            {
                return BadRequest(new { message = "This order has already been processed" });
            }

            if (order.ResponseDeadline.HasValue && DateTime.UtcNow > order.ResponseDeadline.Value)
            {
                return BadRequest(new { message = "The 30-minute deadline for accepting this order has passed" });
            }

            order.Status = OrderStatus.Accepted;
            order.EstimatedDeliveryDate = request?.EstimatedDeliveryDate ?? DateTime.UtcNow.AddDays(3);
            order.UpdatedAt = DateTime.UtcNow;

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

            return Ok(new { message = "Order accepted successfully", orderId = order.Id });
        }

        /// <summary>
        /// Reject an order
        /// </summary>
        [HttpPost("orders/{id}/reject")]
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest(new { message = "Rejection reason is required" });
            }

            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can reject orders");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status != OrderStatus.Pending)
            {
                return BadRequest(new { message = "This order has already been processed" });
            }

            order.Status = OrderStatus.Rejected;
            order.RejectionReason = request.RejectionReason;
            order.UpdatedAt = DateTime.UtcNow;

            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Rejected",
                Message = $"Your order #{order.OrderNumber} has been rejected. Reason: {request.RejectionReason}",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order rejected successfully", orderId = order.Id });
        }

        /// <summary>
        /// Mark order as shipped
        /// </summary>
        [HttpPost("orders/{id}/mark-shipped")]
        public async Task<IActionResult> MarkShipped(int id, [FromBody] MarkShippedRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can mark orders as shipped");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status != OrderStatus.Accepted)
            {
                return BadRequest(new { message = "Only accepted orders can be marked as shipped" });
            }

            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = request.TrackingNumber;
            order.ShippedDate = DateTime.UtcNow;
            order.EstimatedDeliveryDate = request.EstimatedDeliveryDate;
            order.UpdatedAt = DateTime.UtcNow;

            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Shipped",
                Message = $"Your order #{order.OrderNumber} has been shipped. Tracking number: {request.TrackingNumber ?? "N/A"}",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order marked as shipped successfully", orderId = order.Id });
        }

        /// <summary>
        /// Mark order as arrived at delivery location
        /// </summary>
        [HttpPost("orders/{id}/mark-arrived")]
        public async Task<IActionResult> MarkAsArrived(int id)
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can mark orders as arrived");
            }

            var order = await _context.Orders
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id && o.FarmerId == userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (order.Status != OrderStatus.Shipped)
            {
                return BadRequest(new { message = "Only shipped orders can be marked as arrived" });
            }

            order.Status = OrderStatus.Arrived;
            order.ArrivedDate = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            var notification = new Notification
            {
                UserId = order.BuyerId,
                Title = "Order Arrived",
                Message = $"Your order #{order.OrderNumber} has arrived at the delivery location. Please confirm delivery when you receive it.",
                Type = NotificationType.Order,
                IsRead = false,
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order marked as arrived successfully", orderId = order.Id });
        }

        /// <summary>
        /// Get farmer earnings
        /// </summary>
        [HttpGet("earnings")]
        public async Task<ActionResult<EarningsDto>> GetEarnings()
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var totalEarnings = await _context.Orders
                .Where(o => o.FarmerId == userId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var pendingEarnings = await _context.Orders
                .Where(o => o.FarmerId == userId && (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Arrived))
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentTransactions = await _context.Orders
                .Where(o => o.FarmerId == userId && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.DeliveredDate)
                .Take(10)
                .Select(o => new TransactionDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    Amount = o.TotalAmount,
                    Date = o.DeliveredDate ?? o.UpdatedAt ?? o.CreatedAt,
                    BuyerName = $"{o.Buyer.FirstName} {o.Buyer.LastName}"
                })
                .ToListAsync();

            return Ok(new EarningsDto
            {
                TotalEarnings = totalEarnings,
                PendingEarnings = pendingEarnings,
                RecentTransactions = recentTransactions
            });
        }

        /// <summary>
        /// Get farmer profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<FarmerProfileDto>> GetProfile()
        {
            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can access this endpoint");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new FarmerProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                ProfileImage = user.ProfileImage,
                BankAccountNumber = user.BankAccountNumber,
                BankName = user.BankName,
                IsVerified = user.IsVerified
            });
        }

        /// <summary>
        /// Update farmer profile
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<FarmerProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            var userId = GetUserId();
            if (!await IsFarmer(userId))
            {
                return Forbid("Only farmers can update profile");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.City = request.City;
            user.Province = request.Province;
            user.PostalCode = request.PostalCode;
            user.ProfileImage = request.ProfileImage;
            user.BankAccountNumber = request.BankAccountNumber;
            user.BankName = request.BankName;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new FarmerProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                ProfileImage = user.ProfileImage,
                BankAccountNumber = user.BankAccountNumber,
                BankName = user.BankName,
                IsVerified = user.IsVerified
            });
        }
    }

    // DTOs for Farmers API
    public class FarmerDashboardDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; } = new();
    }

    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string? FarmerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DeliveryAddress { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string? DeliveryNotes { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? ArrivedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ResponseDeadline { get; set; }
        public string? RejectionReason { get; set; }
        public int BuyerId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string? BuyerPhone { get; set; }
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UpdateProductRequest
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

    public class AcceptOrderRequest
    {
        public DateTime? EstimatedDeliveryDate { get; set; }
    }

    public class RejectOrderRequest
    {
        [Required(ErrorMessage = "Rejection reason is required")]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class MarkShippedRequest
    {
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }

    public class EarningsDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public List<TransactionDto> RecentTransactions { get; set; } = new();
    }

    public class TransactionDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string BuyerName { get; set; } = string.Empty;
    }

    public class FarmerProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? ProfileImage { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public bool IsVerified { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? ProfileImage { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
    }
}

