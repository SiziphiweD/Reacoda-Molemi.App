using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    public class Notification : BaseModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        public NotificationType Type { get; set; } = NotificationType.Info;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
        
        // Foreign Key
        public int UserId { get; set; }
        
        // Related Entity (for linking notifications to orders, products, etc.)
        public int? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
    
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Order,
        Product,
        System,
        Payment
    }
}
