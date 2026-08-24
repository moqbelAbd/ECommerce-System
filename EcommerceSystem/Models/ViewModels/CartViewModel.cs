namespace EcommerceSystem.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal CartTotal => Items.Sum(i => i.Total);
    }

    public class CartItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}