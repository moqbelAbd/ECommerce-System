using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{

public class CustomerPaymentCard
    {

[Key]
public Guid PaymentCardId { get; set;}

        [MaxLength(500)]
        public string CardNumber { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
public DateOnly CardExpire { get; set; } 

public bool IsDeleted {  get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}
}

}