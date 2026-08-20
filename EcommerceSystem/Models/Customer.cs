namespace EcommerceSystem.Models
{
public class Customer{

[key]
public Guid CustomerId {get; set;}

[Required]
public string firstName   {get; set;}

[Required]
public string lastName   {get; set;}

public string Location    {get; set;}

public bool IsDeleted { get; set; }

public string ApplicationUserId  {get; set;}

[ForeignKey ("ApplicationUserId") ]
public ApplicationUser? ApplicationUser {get; set;}

public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

}

}