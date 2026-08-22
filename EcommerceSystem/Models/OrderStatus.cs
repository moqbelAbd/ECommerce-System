using System.ComponentModel.DataAnnotations;

namespace EcommerceSystem.Models
{
    public class OrderStatus
    {
        [Key]
        public int OrderStatusId { get; set; }

        [Required]
        public string OrderStatusName { get; set; } = string.Empty;
    }
}