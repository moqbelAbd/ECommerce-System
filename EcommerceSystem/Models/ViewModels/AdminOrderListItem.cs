namespace EcommerceSystem.Models.ViewModels
{
    public class AdminOrderListItem
    {
        public Guid OrderId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerPhoneNumber { get; set; }

        public string? Location { get; set; }

        public decimal TotalPrice { get; set; }

        public int PaymentStatusId { get; set; }

        public string PaymentStatusName { get; set; } = string.Empty;

        public int OrderStatusId { get; set; }

        public string OrderStatusName { get; set; } = string.Empty;
    }
}
