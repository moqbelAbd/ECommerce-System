using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class CartItem
    {

        [Key]
        public Guid CartItemId { get; set; }

        [Required]
        public int ItemQuantity { get; set; }

        public Guid CartId { get; set; }

        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public ICollection<Product> OrderProducts { get; set; } = new HashSet<Product>();


    }
}
