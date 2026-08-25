namespace EcommerceSystem.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public Customer Customer { get; set; } = null!;
        public CartViewModel Cart { get; set; } = null!;
    }
}
