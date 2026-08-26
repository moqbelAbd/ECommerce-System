using Microsoft.AspNetCore.Routing;

namespace EcommerceSystem.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public string PageParam { get; set; } = "page";

        public string Action { get; set; } = "Index";

        public string Controller { get; set; } = "Admin";

        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();
    }
}
