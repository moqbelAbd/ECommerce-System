using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductReview
    {
        [Key]
        public Guid ProductReviewId { get; set; }

        [Required]
        [Range(1, 5)]
        public int CustomerProductRating { get; set; }

        [Required]
        [MaxLength(250)]
        public string CustomerProductReview { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Product
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // Customer
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}