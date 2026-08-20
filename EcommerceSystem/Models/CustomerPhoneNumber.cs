using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{

public class CustomerPhoneNumber{

[Key]
public Guid phoneNumberId {get; set;}

[Required]
[MaxLength(50)]
public string PhoneNumber { get; set; }

public bool isDeleted {  get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}
}

}