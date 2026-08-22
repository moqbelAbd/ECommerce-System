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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;

        public CustomerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IDataProtectionProvider protectorProvider)
        {
            _userManager = userManager;
            _context = context;
            _protector = protectorProvider.CreateProtector("EcommerceSystem.PaymentCards.CardNumberKey");
        }

        // GET: Customer
        public async Task<IActionResult> Index(
            Guid? categoryId,
            Guid? subCategoryId)
        {
            var customer = _context.Customers.ToList();
            return View();
        }

        public IActionResult Cart() => View();
        public IActionResult OrderConfirmation() => View();
        public async Task<IActionResult> Checkout()
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
        public IActionResult OrderDetails(int id) => View();

        public IActionResult OrderHistory()
        {
            return View();
        }

        public IActionResult Wishlist()
        {
            return View();
        }

        public IActionResult CompleteProfile() => View();

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

        public async Task<IActionResult> PaymentCards()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                customer = new Customer { ApplicationUserId = userId };
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

        [HttpPost]
        public async Task<IActionResult> AddPaymentCard(string cardHolderName, string cardNumber, DateOnly cardExpire)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                customer = new Customer { CustomerId = Guid.NewGuid(), ApplicationUserId = userId, FirstName = "User", LastName = "" };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                string encryptedCardNumber = _protector.Protect(cardNumber);

                var newCard = new CustomerPaymentCard
                {
                    PaymentCardId = Guid.NewGuid(),
                    CardHolderName = cardHolderName,
                    CardNumber = encryptedCardNumber,
                    CardExpire = cardExpire,
                    CustomerId = customer.CustomerId,
                    IsDeleted = false
                };

                _context.CustomerPaymentCards.Add(newCard);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment card added securely!";
            }

            return RedirectToAction(nameof(PaymentCards));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePaymentCard(Guid cardId)
        {
            var card = await _context.CustomerPaymentCards.FindAsync(cardId);
            if (card != null)
            {
                card.IsDeleted = true;
                _context.CustomerPaymentCards.Update(card);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment card removed successfully!";
            }

            return RedirectToAction(nameof(PaymentCards));
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string firstName, string lastName, string phone, string city, string address, string paymentMethod, Guid? selectedCardId, string newCardHolderName, string newCardNumber, DateOnly? newCardExpire, bool saveNewCard)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null) return RedirectToAction("Index", "Home");

            if (paymentMethod == "Visa" && !selectedCardId.HasValue && !string.IsNullOrWhiteSpace(newCardNumber) && saveNewCard)
            {
                string encryptedCardNumber = _protector.Protect(newCardNumber);
                var newCard = new CustomerPaymentCard
                {
                    PaymentCardId = Guid.NewGuid(),
                    CardHolderName = string.IsNullOrWhiteSpace(newCardHolderName) ? $"{firstName} {lastName}" : newCardHolderName,
                    CardNumber = encryptedCardNumber,
                    CardExpire = newCardExpire ?? DateOnly.FromDateTime(DateTime.Now.AddYears(3)),
                    CustomerId = customer.CustomerId,
                    IsDeleted = false
                };
                _context.CustomerPaymentCards.Add(newCard);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation");
        }
    }
}