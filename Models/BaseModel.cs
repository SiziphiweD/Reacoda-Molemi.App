using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    public abstract class BaseModel
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
