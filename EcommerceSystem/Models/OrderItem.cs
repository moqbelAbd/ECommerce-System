using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }


        [Required]
        public int ItemQuantity { get; set; }

        [Required]
        public decimal ItemTotalPrice { get; set; }


        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Product? Order { get; set; }

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public ICollection<Product> OrderProducts { get; set; } = new HashSet<Product>();

    }
}
