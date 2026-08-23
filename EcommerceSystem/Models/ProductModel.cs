using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductModel
    {

        [Key]
        public Guid ProductModelId { get; set; }

        [Required]
        public string ModelName { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set;} = new HashSet<Product>();

        public Guid ProductBrandId { get; set; }

        [ForeignKey("ProductBrandId")]
        public ProductBrand ProductBrand { get; set; }


    }
}
