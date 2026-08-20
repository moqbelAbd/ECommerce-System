using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class OrderStatus
    {

        [Key]
        public int OrderStatusId { get; set; }

        [Required]
        public string OrderStatusName { get; set; }


    }
}
