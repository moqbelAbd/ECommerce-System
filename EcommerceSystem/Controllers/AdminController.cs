using EcommerceSystem.Data;
using EcommerceSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);

            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
            var totalOrders = await _context.Orders.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted);
            var lowStockCount = await _context.Products
                .CountAsync(p => !p.IsDeleted && p.ProductQuantity <= 5);

            var revenueThisMonth = await _context.Orders
                .Where(o => o.CreatedAt >= startOfThisMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

            var revenueLastMonth = await _context.Orders
                .Where(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt < startOfThisMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

            var ordersThisMonth = await _context.Orders
                .CountAsync(o => o.CreatedAt >= startOfThisMonth);

            var ordersLastMonth = await _context.Orders
                .CountAsync(o => o.CreatedAt >= startOfLastMonth && o.CreatedAt < startOfThisMonth);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.LowStockCount = lowStockCount;

            ViewBag.RevenueChangePercent = revenueLastMonth > 0
                ? Math.Round(((revenueThisMonth - revenueLastMonth) / revenueLastMonth) * 100, 1)
                : (decimal?)null;

            ViewBag.OrdersChangePercent = ordersLastMonth > 0
                ? Math.Round(((decimal)(ordersThisMonth - ordersLastMonth) / ordersLastMonth) * 100, 1)
                : (decimal?)null;

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Customers(string? search)
        {
            var query = _context.Customers
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                query = query.Where(c =>
                    c.FirstName.Contains(term) ||
                    c.LastName.Contains(term) ||
                    (c.Location != null && c.Location.Contains(term)) ||
                    c.CustomerPhoneNumbers.Any(p => !p.IsDeleted && p.PhoneNumber.Contains(term)));
            }

            var customers = await query
                .Select(c => new AdminCustomerListItem
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Location = c.Location,
                    PhoneNumber = c.CustomerPhoneNumbers
                        .Where(p => !p.IsDeleted)
                        .Select(p => p.PhoneNumber)
                        .FirstOrDefault(),
                    OrderCount = c.Orders.Count
                })
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();

            ViewBag.Search = search;

            return View(customers);
        }
    }
}
