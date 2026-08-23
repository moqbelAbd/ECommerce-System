namespace EcommerceSystem.Models.ViewModels
{
    public class AdminCustomerListItem
    {
        public Guid CustomerId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Location { get; set; }

        public string? PhoneNumber { get; set; }

        public int OrderCount { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
