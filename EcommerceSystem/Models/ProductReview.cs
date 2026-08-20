using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductReview
    {

        [Key]
        public Guid ProductReviewId { get; set; }

        [Required]
        public string CustomerProductRating { get; set; }

        [Required]
        [MaxLength(250)]
        public string CustomerProductReview { get; set; }

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
