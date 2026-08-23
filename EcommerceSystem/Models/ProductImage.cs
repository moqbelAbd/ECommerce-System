using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductImage
    {

        [Key]
        public Guid ProductImageId { get; set; }

        [Required]
        public string ProductImagePath { get; set; } = string.Empty;

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
