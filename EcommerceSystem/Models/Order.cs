using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{

public class Order{

[Key]
public Guid OrderId {get; set;}

[Required]
public decimal TotalPrice {get; set;}

public DateTime CreatedAt { get; set; } = DateTime.Now; 

[Required]
public int OrderStatusId { get; set;}

[ForeignKey("OrderStatusId")]
public OrderStatus? OrderStatus { get; set; }

[Required]
public int PaymentStatusId  {get; set;}

[Required]
public string Location { get; set; }


[ForeignKey("PaymentStatusId")]
public PaymentStatus? PaymentStatus { get; set; }

[Required]
public int PaymentTypeId {get; set;}

[ForeignKey("PaymentTypeId")]
public PaymentType? PaymentType { get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}

public ICollection<OrderItem> OrderItems {get; set;} = new HashSet<OrderItem>();

}

}