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
                .OnDelete(DeleteBehavior.Restrict); // <-- THE FIX

        }
    }
}
