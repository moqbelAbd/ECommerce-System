using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    namespace EcommerceSystem.Models
{
    public class Product
    {

        [Key]
        public Guid ProductId { get; set; }

        [MaxLength(255)]
        public string? ProductDescription { get; set; }

        public decimal ProductPrice { get; set; }

        public int ProductQuantity { get; set; }

        public Guid ProductBrandId { get; set; }

        [ForeignKey("ProductBrandId")]
        public ProductBrand? ProductBrand { get; set; }

        public Guid ProductModelId { get; set; }

        [ForeignKey("ProductModelId")]
        public ProductModel? ProductModel { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new HashSet<ProductImage>();

    }
}
