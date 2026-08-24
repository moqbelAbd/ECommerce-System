using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EcommerceSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;

        public CustomerController(ApplicationDbContext context, IDataProtectionProvider protectorProvider)
        {
            _context = context;
            _protector = protectorProvider.CreateProtector("EcommerceSystem.PaymentCards.CardNumberKey");
        }

        // =========================================================
        // PUBLIC SHOP
        // Guest + Customer
        // =========================================================

        public async Task<IActionResult> Index(
            Guid? categoryId,
            Guid? subCategoryId,
            string? searchTerm)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted)

                .Include(p => p.ProductBrand)

                .Include(p => p.ProductModel)

                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)

                .AsQueryable();


            // =====================================================
            // Category Filter
            // =====================================================

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.CategoryId == categoryId.Value));
            }


            // =====================================================
            // SubCategory Filter
            // =====================================================

            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategoryId == subCategoryId.Value));
            }


            // =====================================================
            // Search
            // =====================================================

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm)

                    ||

                    (p.ProductDescription != null &&
                     p.ProductDescription.Contains(searchTerm))

                    ||

                    (p.ProductBrand != null &&
                     p.ProductBrand.BrandName.Contains(searchTerm))

                    ||

                    (p.ProductModel != null &&
                     p.ProductModel.ModelName.Contains(searchTerm))

                    ||

                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.SubCategoryName.Contains(searchTerm))
                );
            }


            // =====================================================
            // Get Products
            // =====================================================

            var products = await query
                .OrderBy(p => p.ProductName)
                .ToListAsync();


            // =====================================================
            // Get Categories + SubCategories
            // =====================================================

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .ToListAsync();


            // =====================================================
            // ViewBag
            // =====================================================

            ViewBag.Categories = categories;

            ViewBag.SelectedCategoryId = categoryId;

            ViewBag.SelectedSubCategoryId = subCategoryId;

            ViewBag.SearchTerm = searchTerm;


            return View(products);
        }


        // =========================================================
        // PUBLIC PRODUCT DETAILS
        // Guest + Customer
        // =========================================================

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();


            var product = await _context.Products
                .Where(p => !p.IsDeleted)

                .Include(p => p.ProductBrand)

                .Include(p => p.ProductModel)

                .Include(p => p.ProductImages)

                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.Customer)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)

                .FirstOrDefaultAsync(p =>
                    p.ProductId == id.Value);


            if (product == null)
                return NotFound();


            return View(product);
        }


        // =========================================================
        // CUSTOMER ONLY
        // =========================================================

        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (userId == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }


            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId);


            if (customer == null)
            {
                return RedirectToAction("CompleteProfile");
            }


            return View(customer);
        }


        // =========================================================
        // ORDER DETAILS
        // =========================================================

        [Authorize]
        public IActionResult OrderDetails(int id)
        {
            return View();
        }


        // =========================================================
        // COMPLETE PROFILE
        // =========================================================

        [Authorize]
        [HttpGet]
        public IActionResult CompleteProfile()
        {
            return View();
        }


        // =========================================================
        // COMPLETE PROFILE POST
        // =========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(string firstName, string lastName, string location)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (existingCustomer != null)
            {
                // If they already have a profile, just try merging their guest cart anyway and send them home
                await MergeSessionCartToDatabaseAsync(existingCustomer.CustomerId);
                return RedirectToAction("Index", "Home");
            }

            // 1. Create the brand new profile
            var customer = new Customer
            {
                ApplicationUserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Location = location,
                IsDeleted = false
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(); // <-- customer.CustomerId is generated right here!

            // 2. NOW IT IS SAFE TO MERGE THE CART!
            await MergeSessionCartToDatabaseAsync(customer.CustomerId);

            return RedirectToAction("Index", "Home");
        }

        // -----------------------------------------------------------
        // THE NEW HELPER METHOD (Place this inside the same Controller)
        // -----------------------------------------------------------
        private async Task MergeSessionCartToDatabaseAsync(Guid customerId)
        {
            // 1. Check if there is anything in the guest cart
            var sessionCartStr = HttpContext.Session.GetString("GuestCart");
            if (string.IsNullOrEmpty(sessionCartStr)) return;

            var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
            if (sessionItems == null || !sessionItems.Any()) return;

            // 2. Find or Create their Database Cart using their new CustomerId
            var dbCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (dbCart == null)
            {
                dbCart = new Cart { CustomerId = customerId };
                _context.Carts.Add(dbCart);
            }

            // 3. Merge the items
            foreach (var sessionItem in sessionItems)
            {
                var existingDbItem = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId);
                if (existingDbItem != null)
                {
                    existingDbItem.ItemQuantity += sessionItem.Quantity;
                }
                else
                {
                    dbCart.CartItems.Add(new CartItem
                    {
                        ProductId = sessionItem.ProductId,
                        ItemQuantity = sessionItem.Quantity
                    });
                }
            }

            // 4. Save changes
            await _context.SaveChangesAsync();

            // 5. CLEAR THE SESSION CART
            HttpContext.Session.Remove("GuestCart");
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cartViewModel = new CartViewModel();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);


            if (cart != null)
            {
                cartViewModel.Items = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.ProductName,
                    ImageUrl = ci.Product.ProductImages.Select(img => img.ProductImagePath).FirstOrDefault() ?? "/images/products/default-product",
                    Price = ci.Product.ProductPrice,        
                    Quantity = ci.ItemQuantity
                }).ToList();
            }

            else
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(sessionCartStr))
                {
                    var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);

                    foreach (var item in sessionItems!)
                    {
                        var product = await _context.Products
                            .Include(p => p.ProductImages)
                            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                        
                        if (product != null)
                        {
                            cartViewModel.Items.Add(new CartItemViewModel
                            {
                                ProductId = product.ProductId,
                                ProductName = product.ProductName,
                                ImageUrl = product.ProductImages?.FirstOrDefault()?.ProductImagePath ?? "/images/products/default-product.jpg",
                                Price = product.ProductPrice,
                                Quantity = item.Quantity
                            });
                        }
                    }
                }
            }

            return View(cartViewModel);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // جلب العميل مع بطاقات الدفع غير المحذوفة
            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            // فك تشفير أرقام البطاقات لعرضها آمنة وسليمة أثناء إتمام الطلب
            foreach (var card in customer.CustomerPaymentCards)
            {
                try
                {
                    card.CardNumber = _protector.Unprotect(card.CardNumber);
                }
                catch
                {
                    card.CardNumber = "********";
                }
            }

            return View(customer);
        }
        // =========================================================
        // CUSTOMER PAYMENT CARDS
        // =========================================================

        // =========================================================
        // CUSTOMER PAYMENT CARDS
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PaymentCards()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile");
            }
            else
            {
                foreach (var card in customer.CustomerPaymentCards)
                {
                    try
                    {
                        card.CardNumber = _protector.Unprotect(card.CardNumber);
                    }
                    catch
                    {
                        card.CardNumber = "********";
                    }
                }
            }

            return View(customer);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentCard(string cardHolderName, string cardNumber, DateOnly cardExpire)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                // تشفير رقم البطاقة قبل حفظه في قاعدة البيانات لأمان تام
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
        // =========================================================
        // DELETE PAYMENT CARD
        // =========================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        // =========================================================
        // CUSTOMER WISHLIST PAGE
        // =========================================================

        [Authorize]
        [HttpGet]
        public IActionResult Wishlist()
        {
            return View();
        }
    }
}