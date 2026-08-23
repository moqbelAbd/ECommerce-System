using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ProductDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductPrice { get; set; }

        public int ProductQuantity { get; set; }

        public bool IsDeleted { get; set; }


        // Product Brand
        public Guid ProductBrandId { get; set; }

        [ForeignKey("ProductBrandId")]
        public ProductBrand? ProductBrand { get; set; }


        // Product Model
        public Guid ProductModelId { get; set; }

        [ForeignKey("ProductModelId")]
        public ProductModel? ProductModel { get; set; }


        // Product Images
        public ICollection<ProductImage> ProductImages { get; set; }
            = new HashSet<ProductImage>();


        // Product Reviews
        public ICollection<ProductReview> ProductReviews { get; set; }
            = new HashSet<ProductReview>();


        // Product <-> SubCategory
        public ICollection<ProductSubCategory> ProductSubCategories { get; set; }
            = new HashSet<ProductSubCategory>();
    }
}