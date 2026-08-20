using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class ProductModel
    {

        [Key]
        public Guid ProductModelId { get; set; }

        [Required]
        public string ModelName { get; set; }


    }
}
