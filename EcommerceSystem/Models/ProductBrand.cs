using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductBrand
    {

        [Key]
        public Guid ProductBrandId { get; set; }

        [Required]
        public string BrandName { get; set; }


    }
}
