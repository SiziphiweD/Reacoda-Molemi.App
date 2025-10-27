using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    public class ProductReview : BaseModel
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        // Foreign Keys
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        
        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual User Buyer { get; set; } = null!;
    }
}
