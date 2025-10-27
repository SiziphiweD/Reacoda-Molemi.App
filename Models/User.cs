using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    public class User : BaseModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Phone]
        public string? PhoneNumber { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
        
        public string? Province { get; set; }
        
        public string? PostalCode { get; set; }
        
        public UserRole Role { get; set; } = UserRole.Buyer;
        
        public bool IsVerified { get; set; } = false;
        
        public string? ProfileImage { get; set; }
        
        public string? BankAccountNumber { get; set; }
        
        public string? BankName { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Order> OrdersReceived { get; set; } = new List<Order>();
    }
    
    public enum UserRole
    {
        Buyer,
        Farmer,
        Admin,
        SuperAdmin
    }
}
