using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReacodeApp.Models
{
    public class Order : BaseModel
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        public string? DeliveryAddress { get; set; }
        
        public string? DeliveryCity { get; set; }
        
        public string? DeliveryProvince { get; set; }
        
        public string? DeliveryPostalCode { get; set; }
        
        public string? DeliveryNotes { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        
        public string? PaymentReference { get; set; }
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? ArrivedDate { get; set; }  // When farmer arrives at delivery location
        
        public DateTime? DeliveredDate { get; set; }  // When buyer confirms delivery
        
        public string? TrackingNumber { get; set; }
        
        // Whether buyer has confirmed delivery
        public bool IsDeliveryConfirmed { get; set; } = false;
        
        // Deadline for farmer to accept/reject (30 minutes from creation)
        public DateTime? ResponseDeadline { get; set; }
        
        // Rejection reason if order is rejected
        public string? RejectionReason { get; set; }
        
        // Estimated delivery date
        public DateTime? EstimatedDeliveryDate { get; set; }
        
        // Foreign Keys
        public int BuyerId { get; set; }
        public int FarmerId { get; set; }
        
        // Navigation properties
        public virtual User Buyer { get; set; } = null!;
        public virtual User Farmer { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Rejected,
        Shipped,
        Arrived,  // Farmer has arrived at delivery location
        Delivered,  // Buyer has confirmed delivery
        Cancelled
    }
    
    public enum PaymentMethod
    {
        CashOnDelivery,
        CreditCard,
        BankTransfer,
        Wallet,
        Stripe
    }
}
