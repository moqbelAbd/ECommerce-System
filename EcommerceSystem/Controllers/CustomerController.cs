using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;

namespace EcommerceSystem.Controllers
{
    [Authorize]
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
            var customer = _context.Customers.ToList();
            return View();
        }

        public IActionResult OrderHistory()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            // ≈–« ·„ Ìﬂ‰ «·⁄„Ì· „ÊÃÊœ«° ﬁÊ„Ì »≈‰‘«¡ ﬂ«∆‰ „ƒﬁ  √Ê  ÊÃÌÂÂ · ﬂ„·… «·»—Ê›«Ì·
            if (customer == null)
            {
                customer = new Customer { ApplicationUserId = userId, CustomerPaymentCards = new List<CustomerPaymentCard>() };
            }
            else
            {
                foreach (var card in customer.CustomerPaymentCards)
                {
                    try { card.CardNumber = _protector.Unprotect(card.CardNumber); }
                    catch { card.CardNumber = "********"; }
                }
            }

            return View(customer);
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

        public IActionResult CompleteProfile() => View();

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