using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace EcommerceSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Shop(string category)
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.CustomerPhoneNumbers.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                customer = new Customer
                {
                    ApplicationUserId = userId,
                    FirstName = appUser.UserName ?? "Guest User",
                    LastName = "",
                    Location = ""
                };
            }

            ViewBag.Email = appUser.Email;
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string location, string newPhoneNumber)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.CustomerPhoneNumbers)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                customer = new Customer
                {
                    ApplicationUserId = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    Location = location
                };
                _context.Customers.Add(customer);
            }
            else
            {
                customer.FirstName = firstName;
                customer.LastName = lastName;
                customer.Location = location;
            }

            if (!string.IsNullOrWhiteSpace(newPhoneNumber))
            {
                _context.CustomerPhoneNumbers.Add(new CustomerPhoneNumber
                {
                    PhoneNumberId = Guid.NewGuid(),
                    PhoneNumber = newPhoneNumber,
                    CustomerId = customer.CustomerId,
                    IsDeleted = false
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoneNumber(Guid phoneId)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            var phone = await _context.CustomerPhoneNumbers.FindAsync(phoneId);
            if (phone != null)
            {
                phone.IsDeleted = true;
                _context.CustomerPhoneNumbers.Update(phone);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile");
        }
    }
}