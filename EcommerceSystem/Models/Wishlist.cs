using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class Wishlist
    {

        [Key]
        public Guid WishlistId { get; set; }

        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }


        public ICollection<WishlistItem> CartItems { get; set; } = new HashSet<WishlistItem>();
    }
}
