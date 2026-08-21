
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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cart()
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
        // Shows the form to the newly registered user
        [HttpGet]
        public IActionResult CompleteProfile()
        {
            return View();
        }

        // POST: Customer/CompleteProfile
        // Saves their data and creates the Customer record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(string firstName, string lastName, string location)
        {
            // Grab the ApplicationUserId of the person who just registered and logged in
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
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

            // Redirect them to the homepage (or wherever you want them to go next)
            return RedirectToAction("Index", "Home");
        }
    }
}


