using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{

public class CustomerPhoneNumber{

[Key]
public Guid PhoneNumberId {get; set;}

[Required]
[MaxLength(20)]
public string PhoneNumber { get; set; } = string.Empty;

public bool IsDeleted {  get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}
}

}