using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductBrand
    {

        [Key]
        public Guid ProductBrandId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = String.Empty;
        public ICollection<Product> Products { get; set; }
    = new HashSet<Product>();


    }
}
