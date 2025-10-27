using System.ComponentModel.DataAnnotations;

namespace ReacodeApp.Models
{
    // Shop ViewModels
    public class ShopIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    // Farmer ViewModels
    public class FarmerDashboardViewModel
    {
        public User Farmer { get; set; } = new();
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalEarnings { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();
    }

    public class AddProductViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerKg { get; set; }
        
        [Required]
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int AvailableQuantity { get; set; }
        
        public string? Location { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public DateTime? HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ImageUrl { get; set; }
        
        public List<Category> Categories { get; set; } = new();
    }

    public class EditProductViewModel
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [System.ComponentModel.DataAnnotations.Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerKg { get; set; }
        
        [Required]
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int AvailableQuantity { get; set; }
        
        public string? Location { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        public DateTime? HarvestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        
        public List<Category> Categories { get; set; } = new();
    }

    public class FarmerEarningsViewModel
    {
        public User Farmer { get; set; } = new();
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public List<Order> RecentTransactions { get; set; } = new();
    }

    // Buyer ViewModels
    public class BuyerDashboardViewModel
    {
        public User Buyer { get; set; } = new();
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> FavoriteProducts { get; set; } = new();
        public List<Product> SuggestedProducts { get; set; } = new();
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = new();
        public List<Product> RelatedProducts { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class CheckoutViewModel
    {
        public User Buyer { get; set; } = new();
        public List<CartItem> CartItems { get; set; } = new();
        
        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;
        
        [Required]
        public string DeliveryCity { get; set; } = string.Empty;
        
        [Required]
        public string DeliveryProvince { get; set; } = string.Empty;
        
        [Required]
        public string DeliveryPostalCode { get; set; } = string.Empty;
        
        public string? DeliveryNotes { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
    }

    // Admin ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalFarmers { get; set; }
        public int TotalBuyers { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalProducts { get; set; }
        public int PendingProductApprovals { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();
    }

    public class AdminReportsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int NewUsers { get; set; }
        public int NewProducts { get; set; }
        public List<SalesByMonth> SalesByMonth { get; set; } = new();
        public List<TopProduct> TopProducts { get; set; } = new();
    }

    public class SalesByMonth
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Sales { get; set; }
    }

    public class TopProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public List<CategorySales> SalesByCategory { get; set; } = new();
    }

    public class CategorySales
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
    }


    public class AuditLogEntry
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
    }

    public class AnalyticsViewModel
    {
        public List<DateValuePair> UserGrowth { get; set; } = new();
        public List<DateValuePair> RevenueGrowth { get; set; } = new();
        public List<CategoryAnalytics> TopCategories { get; set; } = new();
    }

    public class DateValuePair
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    public class CategoryAnalytics
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
    }

    // SuperAdmin ViewModels
    public class SuperAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalFarmers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();
        public List<dynamic> UserGrowthData { get; set; } = new();
        public List<dynamic> RevenueData { get; set; } = new();
    }

    public class SystemConfigurationViewModel
    {
        public decimal PlatformCommissionRate { get; set; }
        public bool PaymentGatewayEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public string PlatformName { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
        public int MaxProductImages { get; set; }
        public int OrderTimeoutHours { get; set; }
    }

    public class GlobalAnalyticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopProductAnalytics> TopProducts { get; set; } = new();
        public List<UserGrowthData> UserGrowthByMonth { get; set; } = new();
        public List<RevenueData> RevenueByMonth { get; set; } = new();
    }

    public class TopProductAnalytics
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class UserGrowthData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int NewUsers { get; set; }
    }

    public class RevenueData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueManagementViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal FarmerEarnings { get; set; }
        public int TotalTransactions { get; set; }
        public decimal CommissionRate { get; set; }
        public List<RevenueByMonth> RevenueByMonth { get; set; } = new();
    }

    public class RevenueByMonth
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
    }

    public class PlatformSettingsViewModel
    {
        public string PlatformName { get; set; } = string.Empty;
        public string PlatformDescription { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string SecondaryColor { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string FaviconUrl { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string SocialMediaFacebook { get; set; } = string.Empty;
        public string SocialMediaTwitter { get; set; } = string.Empty;
        public string SocialMediaInstagram { get; set; } = string.Empty;
    }
}
