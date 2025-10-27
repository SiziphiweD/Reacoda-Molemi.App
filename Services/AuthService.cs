using Microsoft.EntityFrameworkCore;
using ReacodeApp.Data;
using ReacodeApp.Models;
using System.Security.Cryptography;
using System.Text;

namespace ReacodeApp.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<bool> UserExistsAsync(string email);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> RegisterAsync(RegisterViewModel model)
        {
            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = model.SelectedRole,
                IsActive = true,
                IsVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
