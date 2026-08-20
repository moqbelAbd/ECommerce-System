using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models


{
public class Customer{

[Key]
public Guid CustomerId {get; set;}

[Required]
public string firstName   {get; set;}

[Required]
public string lastName   {get; set;}

public string Location    {get; set;}

public bool IsDeleted { get; set; }

        public string? ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public EcommerceSystem.Data.ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

}

}