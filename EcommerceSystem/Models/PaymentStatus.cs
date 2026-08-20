using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class PaymentStatus
    {

        [Key]
        public int PaymentStatusId { get; set; }

        [Required]
        public string PaymentStatusName { get; set; }

    }
}
