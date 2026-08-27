namespace EcommerceSystem.Models.ViewModels
{
    /// <summary>
    /// Aggregated data for the Admin dashboard "Overview" tab (KPIs + charts).
    /// </summary>
    public class AdminOverviewViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int LowStockCount { get; set; }

        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public int OrdersThisMonth { get; set; }
        public int OrdersLastMonth { get; set; }

        public decimal? RevenueChangePercent { get; set; }
        public decimal? OrdersChangePercent { get; set; }

        public decimal AvgOrderValue { get; set; }

        public List<MonthlyStat> MonthlySales { get; set; } = new();
        public List<CategoryRevenue> RevenueByCategory { get; set; } = new();
        public List<StatusSlice> OrdersByStatus { get; set; } = new();

        public class MonthlyStat
        {
            public string Label { get; set; } = string.Empty;
            public int OrderCount { get; set; }
            public decimal Revenue { get; set; }
        }

        public class CategoryRevenue
        {
            public string Name { get; set; } = string.Empty;
            public decimal Revenue { get; set; }
        }

        public class StatusSlice
        {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
}
