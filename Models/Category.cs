using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    public class Category : BaseModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? IconUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
