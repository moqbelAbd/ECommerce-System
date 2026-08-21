using EcommerceSystem.Data;
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
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => !c.IsDeleted);
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
