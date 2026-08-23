using EcommerceSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
     : IdentityDbContext<ApplicationUser>(options)
    {
        // 1. User & Account Tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerPhoneNumber> CustomerPhoneNumbers { get; set; }
        public DbSet<CustomerPaymentCard> CustomerPaymentCards { get; set; }

        // 2. Product Catalog Tables
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<ProductSubCategory> ProductSubCategories { get; set; } 
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductModel> ProductModels { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        // 3. Cart & Wishlist Tables (Added!)
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        // 4. Order & Checkout Tables
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; } 
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<PaymentStatus> PaymentStatuses { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }

        // 5. Misc Tables (Added!)
        public DbSet<Testimonial> Testimonials { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. MUST call the base method first so ASP.NET Identity tables build correctly!
            base.OnModelCreating(modelBuilder);

            // 2. Prevent SQL Server from cascade deleting OrderItems when a Product is deleted
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // Leaves the other side of the relationship empty
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductModel>()
            .HasOne(pm => pm.ProductBrand)
            .WithMany(pb => pb.ProductModels)
            .HasForeignKey(pm => pm.ProductBrandId)
            .OnDelete(DeleteBehavior.Restrict);

            // 3. Explicit precision for decimal columns to avoid silent truncation
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.ItemTotalPrice)
                .HasPrecision(18, 2);

            // 1. Seed Product Brands
            modelBuilder.Entity<ProductBrand>().HasData(
                new ProductBrand { ProductBrandId = Guid.Parse("b1111111-1111-1111-1111-111111111111"), BrandName = "Rolex" },     // Luxury Watch
                new ProductBrand { ProductBrandId = Guid.Parse("b2222222-2222-2222-2222-222222222222"), BrandName = "Tissot" },    // Classic Watch
                new ProductBrand { ProductBrandId = Guid.Parse("b3333333-3333-3333-3333-333333333333"), BrandName = "Casio" },     // Sport Watch
                new ProductBrand { ProductBrandId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), BrandName = "Apple" },     // Smartwatch
                new ProductBrand { ProductBrandId = Guid.Parse("b5555555-5555-5555-5555-555555555555"), BrandName = "eontblanc" }  // Accessories
            );

            // 2. Seed Product Models
            modelBuilder.Entity<ProductModel>().HasData(
                new ProductModel { ProductModelId = Guid.Parse("e1111111-1111-1111-1111-111111111111"), ProductBrandId = Guid.Parse("b1111111-1111-1111-1111-111111111111"), ModelName = "Submariner" },
                new ProductModel { ProductModelId = Guid.Parse("e1111111-2222-2222-2222-222222222222"), ProductBrandId = Guid.Parse("b1111111-1111-1111-1111-111111111111"), ModelName = "Datejust" },

                new ProductModel { ProductModelId = Guid.Parse("e2222222-1111-1111-1111-111111111111"), ProductBrandId = Guid.Parse("b2222222-2222-2222-2222-222222222222"), ModelName = "Le Locle" },
                new ProductModel { ProductModelId = Guid.Parse("e2222222-2222-2222-2222-222222222222"), ProductBrandId = Guid.Parse("b2222222-2222-2222-2222-222222222222"), ModelName = "PRX" },

                new ProductModel { ProductModelId = Guid.Parse("e3333333-1111-1111-1111-111111111111"), ProductBrandId = Guid.Parse("b3333333-3333-3333-3333-333333333333"), ModelName = "G-Shock" },
                new ProductModel { ProductModelId = Guid.Parse("e3333333-2222-2222-2222-222222222222"), ProductBrandId = Guid.Parse("b3333333-3333-3333-3333-333333333333"), ModelName = "Edifice" },

                new ProductModel { ProductModelId = Guid.Parse("e4444444-1111-1111-1111-111111111111"), ProductBrandId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), ModelName = "Series 9" },
                new ProductModel { ProductModelId = Guid.Parse("e4444444-2222-2222-2222-222222222222"), ProductBrandId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), ModelName = "Ultra 2" },

                new ProductModel { ProductModelId = Guid.Parse("e5555555-1111-1111-1111-111111111111"), ProductBrandId = Guid.Parse("b5555555-5555-5555-5555-555555555555"), ModelName = "Meisterstück" },
                new ProductModel { ProductModelId = Guid.Parse("e5555555-2222-2222-2222-222222222222"), ProductBrandId = Guid.Parse("b5555555-5555-5555-5555-555555555555"), ModelName = "Sartorial" }
                );

            // 3. Seed Payment Types (Integers instead of Guids!)
            modelBuilder.Entity<PaymentType>().HasData(
                new PaymentType { PaymentTypeId = 1, PaymentTypeName = "Credit Card" },
                new PaymentType { PaymentTypeId = 2, PaymentTypeName = "PayPal" },
                new PaymentType { PaymentTypeId = 3, PaymentTypeName = "Cash on Delivery (COD)" }
            );

            // 4. Seed Order Statuses
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { OrderStatusId = 1, OrderStatusName = "Pending" },
                new OrderStatus { OrderStatusId = 2, OrderStatusName = "Processing" },
                new OrderStatus { OrderStatusId = 3, OrderStatusName = "Shipped" },
                new OrderStatus { OrderStatusId = 4, OrderStatusName = "Delivered" },
                new OrderStatus { OrderStatusId = 5, OrderStatusName = "Cancelled" }
            );

            // 5. Seed Payment Statuses
            modelBuilder.Entity<PaymentStatus>().HasData(
                new PaymentStatus { PaymentStatusId = 1, PaymentStatusName = "Pending" },
                new PaymentStatus { PaymentStatusId = 2, PaymentStatusName = "Completed" },
                new PaymentStatus { PaymentStatusId = 3, PaymentStatusName = "Failed" },
                new PaymentStatus { PaymentStatusId = 4, PaymentStatusName = "Refunded" }
            );
        }


    }


}
