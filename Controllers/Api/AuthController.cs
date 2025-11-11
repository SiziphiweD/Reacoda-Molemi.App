using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using ReacodeApp.Services;

namespace ReacodeApp.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthController> _logger;
        private readonly ISessionService _sessionService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger, ISessionService sessionService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
            _logger = logger;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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

            // Check if user already exists (case-insensitive email comparison)
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            _logger.LogInformation("Checking if email exists: {Email} (normalized: {NormalizedEmail})", request.Email, normalizedEmail);
            
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email already exists. Email: {Email}, Existing User ID: {UserId}", 
                    request.Email, existingUser.Id);
                return BadRequest(new { 
                    message = "Email already registered",
                    details = $"Email '{request.Email}' is already registered with User ID: {existingUser.Id}"
                });
            }
            
            _logger.LogInformation("Email {Email} is available for registration", request.Email);

            // Parse role from string to enum
            if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var userRole))
            {
                _logger.LogWarning("Invalid role provided: {Role}, defaulting to Buyer", request.Role);
                userRole = UserRole.Buyer;
            }

            // Create new user
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = userRole,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _logger.LogInformation("Password hashed successfully for user: {Email}", user.Email);

            // Add to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User registered successfully. User ID: {UserId}, Email: {Email}", user.Id, user.Email);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "User registered successfully",
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString()
                }
            });
        }

        /// <summary>
        /// Login and get JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
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

            // Find user by email (case-insensitive)
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            _logger.LogInformation("Login attempt for email: {Email} (normalized: {NormalizedEmail})", request.Email, normalizedEmail);
            
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found. Email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            _logger.LogInformation("User found. User ID: {UserId}, Email: {Email}, IsActive: {IsActive}", 
                user.Id, user.Email, user.IsActive);

            // Verify password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            _logger.LogInformation("Password verification result: {Result} for User ID: {UserId}", 
                passwordResult, user.Id);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login failed: Invalid password for User ID: {UserId}, Email: {Email}", 
                    user.Id, user.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString()
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ReacodeApp";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "ReacodeAppUsers";
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Get JWT token for logged-in web user (from session)
        /// </summary>
        [HttpGet("token")]
        public IActionResult GetToken()
        {
            // Check if user is logged in via session
            if (!_sessionService.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in" });
            }

            var user = _sessionService.GetUser();
            if (user == null)
            {
                return Unauthorized(new { message = "User not found in session" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role.ToString()
                }
            });
        }
    }

    // Request DTOs
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        // Accept role as string and convert to enum
        public string Role { get; set; } = "Buyer";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

