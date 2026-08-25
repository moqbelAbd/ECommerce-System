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
        public async Task<IActionResult> Index(
            string? activeTab,
            string? customerSearch,
            string? orderSearch,
            int? orderStatusId,
            int? paymentStatusId)
        {
            var now = DateTime.Now;
            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);

            ViewBag.TotalSubCategories = await _context.SubCategories.CountAsync(sc => !sc.IsDeleted);

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

            var customerQuery = _context.Customers
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerSearch))
            {
                var term = customerSearch.Trim();

                customerQuery = customerQuery.Where(c =>
                    c.FirstName.Contains(term) ||
                    c.LastName.Contains(term) ||
                    (c.Location != null && c.Location.Contains(term)) ||
                    c.CustomerPhoneNumbers.Any(p => !p.IsDeleted && p.PhoneNumber.Contains(term)));
            }

            var customers = await customerQuery
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

            var orderQuery = _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.CustomerPhoneNumbers)
                .Include(o => o.PaymentStatus)
                .Include(o => o.OrderStatus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(orderSearch))
            {
                var term = orderSearch.Trim();

                orderQuery = orderQuery.Where(o =>
                    (o.Customer != null &&
                        ((o.Customer.FirstName + " " + o.Customer.LastName).Contains(term) ||
                         o.Customer.CustomerPhoneNumbers.Any(p => !p.IsDeleted && p.PhoneNumber.Contains(term)))) ||
                    (o.Location != null && o.Location.Contains(term)));
            }

            if (orderStatusId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderStatusId == orderStatusId.Value);
            }

            if (paymentStatusId.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            }

            var orders = await orderQuery
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new AdminOrderListItem
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer == null
                        ? "-"
                        : (o.Customer.FirstName + " " + o.Customer.LastName),
                    CustomerPhoneNumber = o.Customer == null
                        ? null
                        : o.Customer.CustomerPhoneNumbers
                            .Where(p => !p.IsDeleted)
                            .Select(p => p.PhoneNumber)
                            .FirstOrDefault(),
                    Location = o.Location,
                    TotalPrice = o.TotalPrice,
                    PaymentStatusId = o.PaymentStatusId,
                    PaymentStatusName = o.PaymentStatus != null
                        ? o.PaymentStatus.PaymentStatusName
                        : "-",
                    OrderStatusId = o.OrderStatusId,
                    OrderStatusName = o.OrderStatus != null
                        ? o.OrderStatus.OrderStatusName
                        : "-"
                })
                .ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.CustomerSearch = customerSearch;

            ViewBag.Orders = orders;
            ViewBag.OrderStatuses = await _context.OrderStatuses.OrderBy(s => s.OrderStatusId).ToListAsync();
            ViewBag.PaymentStatuses = await _context.PaymentStatuses.OrderBy(s => s.PaymentStatusId).ToListAsync();
            ViewBag.OrderSearch = orderSearch;
            ViewBag.SelectedOrderStatusId = orderStatusId;
            ViewBag.SelectedPaymentStatusId = paymentStatusId;

            ViewBag.ActiveTab = string.IsNullOrWhiteSpace(activeTab) ? "orders" : activeTab;

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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Testimonials()
        {
            var testimonials = await _context.Testimonials
                .Include(t => t.Customer)
                .OrderByDescending(t => t.TestimonialId)
                .ToListAsync();

            return View(testimonials);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTestimonialApproval(Guid id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                testimonial.IsApproved = !testimonial.IsApproved;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Testimonial status updated successfully.";
            }
            return RedirectToAction(nameof(Testimonials));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestimonial(Guid id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Testimonial deleted successfully.";
            }
            return RedirectToAction(nameof(Testimonials));
        }
    }
}
