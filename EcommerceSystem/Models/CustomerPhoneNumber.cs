namespace EcommerceSystem.Models
{

public class CustomerPhoneNumber{

[Key]
public Guid phoneNumberId {get; set;}

[Required]
[MaxLength(50)]
public string PhoneNumber { get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}
}

}