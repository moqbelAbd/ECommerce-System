using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models


{
public class Customer{

[Key]
public Guid CustomerId {get; set;}

[Required]
public string FirstName   {get; set;} = string.Empty;

[Required]
public string LastName   {get; set;} = string.Empty;

public string? Location    {get; set;}

public bool IsDeleted { get; set; }

public string? ApplicationUserId { get; set; }

[ForeignKey("ApplicationUserId")]
public EcommerceSystem.Data.ApplicationUser? ApplicationUser { get; set; }

public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

}

}