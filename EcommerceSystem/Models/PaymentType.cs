using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSystem.Models
{
    public class PaymentType
    {

        [Key]
        public int PaymentTypeId { get; set; }

        [Required]
        public string PaymentTypeName { get; set; }

    }
}
