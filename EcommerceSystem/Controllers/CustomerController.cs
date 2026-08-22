using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(
            Guid? categoryId,
            Guid? subCategoryId)
        {
            return View();
        }

        public IActionResult OrderHistory()
        {
            return View();
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        public IActionResult Wishlist()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }

        public IActionResult OrderDetails(int id)
        {
            return View();
        }

        // GET: Customer/CompleteProfile
        [HttpGet]
        public IActionResult CompleteProfile()
        {
            return View();
        }

        // POST: Customer/CompleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(
            string firstName,
            string lastName,
            string location)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }

            var customer = new Customer
            {
                ApplicationUserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Location = location,
                IsDeleted = false
            };

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}