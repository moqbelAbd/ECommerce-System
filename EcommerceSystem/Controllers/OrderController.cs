using EcommerceSystem.Data;
using EcommerceSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index(string? search, int? orderStatusId, int? paymentStatusId)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.CustomerPhoneNumbers)
                .Include(o => o.PaymentStatus)
                .Include(o => o.OrderStatus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                query = query.Where(o =>
                    (o.Customer != null &&
                        ((o.Customer.FirstName + " " + o.Customer.LastName).Contains(term) ||
                         o.Customer.CustomerPhoneNumbers.Any(p => !p.IsDeleted && p.PhoneNumber.Contains(term)))) ||
                    (o.Location != null && o.Location.Contains(term)));
            }

            if (orderStatusId.HasValue)
            {
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            }

            if (paymentStatusId.HasValue)
            {
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            }

            var orders = await query
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

            ViewBag.OrderStatuses = await _context.OrderStatuses
                .OrderBy(s => s.OrderStatusId)
                .ToListAsync();

            ViewBag.PaymentStatuses = await _context.PaymentStatuses
                .OrderBy(s => s.PaymentStatusId)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.SelectedOrderStatusId = orderStatusId;
            ViewBag.SelectedPaymentStatusId = paymentStatusId;

            return View(orders);
        }

        // GET: Order/Preview/{id}
        public async Task<IActionResult> Preview(Guid? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.CustomerPhoneNumbers)
                .Include(o => o.PaymentStatus)
                .Include(o => o.PaymentType)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id.Value);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid orderId, int orderStatusId, string? returnUrl)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            var statusExists = await _context.OrderStatuses
                .AnyAsync(s => s.OrderStatusId == orderStatusId);

            if (!statusExists)
                return NotFound();

            order.OrderStatusId = orderStatusId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order status updated successfully.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
