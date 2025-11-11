using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReacodeApp.Models
{
    public class FarmerRating : BaseModel
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        // Foreign Keys
        [Required]
        public int FarmerId { get; set; }
        
        [Required]
        public int BuyerId { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        // Navigation properties
        public virtual User Farmer { get; set; } = null!;
        public virtual User Buyer { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}





