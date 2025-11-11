using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReacodeApp.Models
{
    public class PaymentTransaction : BaseModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentGateway { get; set; } = string.Empty; // e.g., "Stripe"

        [Required]
        [StringLength(255)]
        public string TransactionId { get; set; } = string.Empty; // Stripe PaymentIntent ID or Session ID

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "ZAR";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // e.g., "succeeded", "failed", "pending"

        public string? RawResponse { get; set; } // Store full JSON response for debugging

        // Navigation property
        public virtual Order Order { get; set; } = null!;
    }
}





