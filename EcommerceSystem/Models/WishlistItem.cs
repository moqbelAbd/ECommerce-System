using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class WishlistItem
    {

        [Key]
        public Guid WishlistItemId { get; set; }

        public Guid WishlistId { get; set; }

        [ForeignKey("WishlistId")]
        public Wishlist? Wishlist { get; set; }

        public Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        //public ICollection<Product> WishlistProducts { get; set; } = new HashSet<Product>();


    }
}
