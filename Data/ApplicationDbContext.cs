using Microsoft.EntityFrameworkCore;
using ReacodeApp.Models;

namespace ReacodeApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all models
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<FarmerRating> FarmerRatings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Products)
                .WithOne(p => p.Farmer)
                .HasForeignKey(p => p.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.Buyer)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.OrdersReceived)
                .WithOne(o => o.Farmer)
                .HasForeignKey(o => o.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order relationships
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProductReview relationships
            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Buyer)
                .WithMany()
                .HasForeignKey(pr => pr.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FarmerRating relationships
            modelBuilder.Entity<FarmerRating>()
                .HasOne(fr => fr.Farmer)
                .WithMany()
                .HasForeignKey(fr => fr.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmerRating>()
                .HasOne(fr => fr.Buyer)
                .WithMany()
                .HasForeignKey(fr => fr.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmerRating>()
                .HasOne(fr => fr.Order)
                .WithMany()
                .HasForeignKey(fr => fr.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FarmerRating>()
                .HasIndex(fr => new { fr.OrderId, fr.BuyerId })
                .IsUnique();

            
            // Configure Favorite relationships
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique favorite per user-product combination
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.ProductId })
                .IsUnique();

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Cart relationships
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique cart item per user-product combination
            modelBuilder.Entity<Cart>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            // Configure PaymentTransaction relationships
            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.Order)
                .WithMany()
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Vegetables", Description = "Fresh vegetables", IsActive = true },
                new Category { Id = 2, Name = "Fruits", Description = "Fresh fruits", IsActive = true },
                new Category { Id = 3, Name = "Grains", Description = "Cereals and grains", IsActive = true },
                new Category { Id = 4, Name = "Herbs", Description = "Fresh herbs and spices", IsActive = true },
                new Category { Id = 5, Name = "Dairy", Description = "Dairy products", IsActive = true }
            );

            // Seed sample farmers
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 10,
                    Email = "farmer1@example.com",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", // farmer123
                    FirstName = "John",
                    LastName = "Smith",
                    PhoneNumber = "+27 82 123 4567",
                    Address = "123 Farm Road",
                    City = "Cape Town",
                    Province = "Western Cape",
                    PostalCode = "8000",
                    Role = UserRole.Farmer,
                    IsVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 11,
                    Email = "farmer2@example.com",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", // farmer123
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    PhoneNumber = "+27 83 987 6543",
                    Address = "456 Harvest Lane",
                    City = "Johannesburg",
                    Province = "Gauteng",
                    PostalCode = "2000",
                    Role = UserRole.Farmer,
                    IsVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = 12,
                    Email = "farmer3@example.com",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", // farmer123
                    FirstName = "Mike",
                    LastName = "Brown",
                    PhoneNumber = "+27 84 555 1234",
                    Address = "789 Green Valley",
                    City = "Durban",
                    Province = "KwaZulu-Natal",
                    PostalCode = "4000",
                    Role = UserRole.Farmer,
                    IsVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed sample products with images
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Fresh Organic Tomatoes",
                    Description = "Premium organic tomatoes grown in our greenhouse. Perfect for salads, cooking, and fresh eating.",
                    PricePerKg = 45.00m,
                    AvailableQuantity = 50,
                    Location = "Cape Town, Western Cape",
                    CategoryId = 1, // Vegetables
                    FarmerId = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1546470427-5bb7c3a1b7b8?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-2),
                    ExpiryDate = DateTime.UtcNow.AddDays(5),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "Sweet Corn",
                    Description = "Fresh sweet corn harvested daily. Great for grilling, boiling, or adding to salads.",
                    PricePerKg = 35.00m,
                    AvailableQuantity = 30,
                    Location = "Cape Town, Western Cape",
                    CategoryId = 1, // Vegetables
                    FarmerId = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1551754655-cd27e38d2076?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-1),
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 3,
                    Name = "Fresh Strawberries",
                    Description = "Sweet, juicy strawberries perfect for desserts, smoothies, or eating fresh.",
                    PricePerKg = 80.00m,
                    AvailableQuantity = 25,
                    Location = "Johannesburg, Gauteng",
                    CategoryId = 2, // Fruits
                    FarmerId = 11,
                    ImageUrl = "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-1),
                    ExpiryDate = DateTime.UtcNow.AddDays(3),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 4,
                    Name = "Organic Apples",
                    Description = "Crisp, sweet organic apples from our orchard. Perfect for snacking or baking.",
                    PricePerKg = 55.00m,
                    AvailableQuantity = 40,
                    Location = "Johannesburg, Gauteng",
                    CategoryId = 2, // Fruits
                    FarmerId = 11,
                    ImageUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-3),
                    ExpiryDate = DateTime.UtcNow.AddDays(14),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 5,
                    Name = "Fresh Basil",
                    Description = "Aromatic fresh basil perfect for Italian dishes, pesto, and garnishing.",
                    PricePerKg = 120.00m,
                    AvailableQuantity = 15,
                    Location = "Durban, KwaZulu-Natal",
                    CategoryId = 4, // Herbs
                    FarmerId = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1615485925442-7b4b8b5b5b5b?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-1),
                    ExpiryDate = DateTime.UtcNow.AddDays(5),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 6,
                    Name = "Fresh Rosemary",
                    Description = "Fragrant rosemary perfect for roasting, grilling, and Mediterranean dishes.",
                    PricePerKg = 100.00m,
                    AvailableQuantity = 20,
                    Location = "Durban, KwaZulu-Natal",
                    CategoryId = 4, // Herbs
                    FarmerId = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1615485925442-7b4b8b5b5b5b?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-2),
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 7,
                    Name = "Organic Quinoa",
                    Description = "Premium organic quinoa, high in protein and perfect for healthy meals.",
                    PricePerKg = 95.00m,
                    AvailableQuantity = 35,
                    Location = "Cape Town, Western Cape",
                    CategoryId = 3, // Grains
                    FarmerId = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-5),
                    ExpiryDate = DateTime.UtcNow.AddDays(365),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 8,
                    Name = "Fresh Carrots",
                    Description = "Sweet, crunchy carrots perfect for snacking, cooking, or juicing.",
                    PricePerKg = 25.00m,
                    AvailableQuantity = 60,
                    Location = "Johannesburg, Gauteng",
                    CategoryId = 1, // Vegetables
                    FarmerId = 11,
                    ImageUrl = "https://images.unsplash.com/photo-1598170845058-32b9d6a5da35?w=400&h=300&fit=crop",
                    HarvestDate = DateTime.UtcNow.AddDays(-2),
                    ExpiryDate = DateTime.UtcNow.AddDays(10),
                    IsAvailable = true,
                    Status = ProductStatus.Approved,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed Admin and Super Admin accounts
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    Email = "admin@reacoda.com", 
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", // admin123
                    FirstName = "System", 
                    LastName = "Admin", 
                    Role = UserRole.Admin, 
                    IsVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Id = 2, 
                    Email = "superadmin@reacoda.com", 
                    PasswordHash = "40+SogUyqHPLMYQ5gHC0uCqPopz0hXLCA9xfD6YVgjE=", // superadmin123
                    FirstName = "Super", 
                    LastName = "Admin", 
                    Role = UserRole.SuperAdmin, 
                    IsVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
