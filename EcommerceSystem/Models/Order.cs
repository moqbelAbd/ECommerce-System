using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EcommerceSystem.Models
{

public class Order{

[Key]
public Guid OrderId {get; set;}

[Required]
public decimal TotalPrice {get; set;}

public DateTime CreatedAt { get; set; } = DateTime.Now; // Automatically sets the current time!

[Required]
public int orderStatus  {get; set;}

//[ForeignKey("OrderStatusId")]
//public OrderStatus? OrderStatus { get; set; }

[Required]
public int paymentStatus  {get; set;}

//[ForeignKey("PaymentStatusId")]
//public PaymentStatus? PaymentStatus { get; set; }

[Required]
public int paymentTypeId {get; set;}

//[ForeignKey("PaymentTypeId")]
//public PaymentType? PaymentType { get; set; }

public Guid CustomerId {get; set;}

[ForeignKey ("CustomerId")]
public Customer? Customer {get; set;}

//public ICollection<OrderItem> OrderItems {get; set;} = new HashSet<OrderItem>();

}

}