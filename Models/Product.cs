using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReacodeApp.Models
{
    public class Product : BaseModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerKg { get; set; }
        
        [Required]
        public int AvailableQuantity { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? Location { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public ProductStatus Status { get; set; } = ProductStatus.Pending;
        
        public DateTime? HarvestDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        // Foreign Keys
        public int FarmerId { get; set; }
        public int CategoryId { get; set; }
        
        // Navigation properties
        public virtual User Farmer { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
    
    public enum ProductStatus
    {
        Pending,
        Approved,
        Rejected,
        OutOfStock
    }
}
