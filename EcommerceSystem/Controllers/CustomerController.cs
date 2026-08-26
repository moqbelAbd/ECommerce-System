using EcommerceSystem.Data;
using EcommerceSystem.Models;
using EcommerceSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
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

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.CategoryId == categoryId.Value));
            }

            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategoryId == subCategoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    (p.ProductDescription != null && p.ProductDescription.Contains(searchTerm)) ||
                    (p.ProductBrand != null && p.ProductBrand.BrandName.Contains(searchTerm)) ||
                    (p.ProductModel != null && p.ProductModel.ModelName.Contains(searchTerm)) ||
                    p.ProductSubCategories.Any(psc => psc.SubCategory != null && psc.SubCategory.SubCategoryName.Contains(searchTerm))
                );
            }

            var products = await query.OrderBy(p => p.ProductName).ToListAsync();

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;
            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }

        // =========================================================
        // PUBLIC PRODUCT DETAILS
        // =========================================================
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        // =========================================================
        // CUSTOMER ONLY - ORDER HISTORY
        // =========================================================
        [Authorize]
        public async Task<IActionResult> OrderHistory(int? statusId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (customer == null) return RedirectToAction("CompleteProfile");

            var query = _context.Orders
                .Include(o => o.OrderStatus)
                .Where(o => o.CustomerId == customer.CustomerId);

            if (statusId.HasValue)
            {
                query = query.Where(o => o.OrderStatusId == statusId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (customer == null) return RedirectToAction("CompleteProfile");

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentStatus)
                .Include(o => o.Customer)
                  .ThenInclude(c => c!.CustomerPhoneNumbers)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customer.CustomerId);

            if (order == null) return NotFound();

            return View(order);
        }

        // =========================================================
        // ORDER CONFIRMATION (Invoice)
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(Guid id)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (customer == null) return RedirectToAction("CompleteProfile");

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentType)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customer.CustomerId);

            if (order == null) return NotFound();

            return View(order);
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(string firstName, string lastName, string location)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (existingCustomer != null)
            {
                await MergeSessionCartToDatabaseAsync(existingCustomer.CustomerId);
                return RedirectToAction("Index", "Home");
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

            await MergeSessionCartToDatabaseAsync(customer.CustomerId);

            return RedirectToAction("Index", "Home");
        }

        private async Task MergeSessionCartToDatabaseAsync(Guid customerId)
        {
            var sessionCartStr = HttpContext.Session.GetString("GuestCart");
            if (string.IsNullOrEmpty(sessionCartStr)) return;

            var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
            if (sessionItems == null || !sessionItems.Any()) return;

            var dbCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (dbCart == null)
            {
                dbCart = new Cart { CustomerId = customerId };
                _context.Carts.Add(dbCart);
            }

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

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("GuestCart");
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cartViewModel = new CartViewModel();

            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
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
                            ImageUrl = ci.Product.ProductImages.FirstOrDefault()?.ProductImagePath ?? "/images/products/default-product.jpg",
                            Price = ci.Product.ProductPrice,
                            Quantity = ci.ItemQuantity
                        }).ToList();
                    }
                }
            }
            else 
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(sessionCartStr))
                {
                    var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
                    if (sessionItems != null)
                    {
                        foreach (var item in sessionItems)
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
                                    ImageUrl = product.ProductImages.FirstOrDefault()?.ProductImagePath ?? "/images/products/default-product.jpg",
                                    Price = product.ProductPrice,
                                    Quantity = item.Quantity
                                });
                            }
                        }
                    }
                }
            }

            return View(cartViewModel);
        }
        // =========================================================
        // CHECKOUT GET
        // =========================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null) return RedirectToAction("CompleteProfile");

            foreach (var card in customer.CustomerPaymentCards)
            {
                try { card.CardNumber = _protector.Unprotect(card.CardNumber); }
                catch { card.CardNumber = "********"; }
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            if (cart == null || !cart.CartItems.Any()) return RedirectToAction("Cart", "Customer");

            var cartViewModel = new CartViewModel
            {
                Items = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.ProductName,
                    ImageUrl = ci.Product.ProductImages.Select(img => img.ProductImagePath).FirstOrDefault() ?? "/images/products/default-product.jpg",
                    Price = ci.Product.ProductPrice,
                    Quantity = ci.ItemQuantity
                }).ToList()
            };

            var viewModel = new CheckoutViewModel
            {
                Customer = customer,
                Cart = cartViewModel
            };

            return View(viewModel);
        }

        // =========================================================
        // PLACE ORDER POST (AJAX)
        // =========================================================
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            if (request == null) return Json(new { success = false, message = "Invalid data format received." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (customer == null) return Json(new { success = false, message = "Customer not found." });

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            if (cart == null || !cart.CartItems.Any()) return Json(new { success = false, message = "Your cart is empty." });

            if (request.Quantities != null)
            {
                foreach (var item in cart.CartItems)
                {
                    if (request.Quantities.ContainsKey(item.ProductId))
                    {
                        item.ItemQuantity = request.Quantities[item.ProductId];
                    }
                }
            }

            if (request.PaymentMethod == "Visa" && !request.SelectedCardId.HasValue)
            {
                if (!request.NewCardExpire.HasValue || request.NewCardExpire.Value.Date < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Card expiration date is invalid or has expired." });
                }
                if (string.IsNullOrWhiteSpace(request.NewCardNumber))
                {
                    return Json(new { success = false, message = "Please provide valid card details." });
                }
            }

            var stockErrors = new List<object>();
            decimal totalPrice = 0;

            foreach (var item in cart.CartItems)
            {
                if (item.ItemQuantity > item.Product!.ProductQuantity)
                {
                    stockErrors.Add(new { productId = item.ProductId, message = $"Only {item.Product.ProductQuantity} in stock" });
                }
                totalPrice += (item.ItemQuantity * item.Product.ProductPrice);
            }

            if (stockErrors.Any()) return Json(new { success = false, message = "Some items exceed available stock.", errors = stockErrors });

            var order = new Order
            {
                CustomerId = customer.CustomerId,
                TotalPrice = totalPrice,
                Location = string.IsNullOrWhiteSpace(request.Location) ? customer.Location : request.Location,
                OrderStatusId = 1,
                PaymentStatusId = 1,
                PaymentTypeId = request.PaymentMethod == "Visa" ? 2 : 1,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);

            foreach (var item in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    ItemQuantity = item.ItemQuantity,
                    ItemTotalPrice = item.ItemQuantity * item.Product!.ProductPrice
                };
                _context.OrderItems.Add(orderItem);
                item.Product.ProductQuantity -= item.ItemQuantity;
            }

            if (request.PaymentMethod == "Visa" && !request.SelectedCardId.HasValue && !string.IsNullOrEmpty(request.NewCardNumber))
            {
                string encryptedCardNumber = _protector.Protect(request.NewCardNumber);
                var newCard = new CustomerPaymentCard
                {
                    PaymentCardId = Guid.NewGuid(),
                    CardHolderName = request.NewCardHolderName ?? string.Empty,
                    CardNumber = encryptedCardNumber,
                    CardExpire = request.NewCardExpire.HasValue ? DateOnly.FromDateTime(request.NewCardExpire.Value) : DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                    CustomerId = customer.CustomerId,
                    IsDeleted = false
                };
                _context.CustomerPaymentCards.Add(newCard);
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order placed successfully!", orderId = order.OrderId });
        }
        public class PlaceOrderRequest
        {
            public string Location { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public Guid? SelectedCardId { get; set; }
            public string? NewCardHolderName { get; set; }
            public string? NewCardNumber { get; set; }
            public DateTime? NewCardExpire { get; set; }
            public string? DummyCVV { get; set; }
            public bool SaveNewCard { get; set; }
            public Dictionary<Guid, int> Quantities { get; set; } = new();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PaymentCards()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

            var customer = await _context.Customers
                .Include(c => c.CustomerPaymentCards.Where(card => !card.IsDeleted))
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null) return RedirectToAction("CompleteProfile");

            foreach (var card in customer.CustomerPaymentCards)
            {
                try { card.CardNumber = _protector.Unprotect(card.CardNumber); }
                catch { card.CardNumber = "********"; }
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
            if (customer == null) return RedirectToAction("CompleteProfile");

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

        [Authorize]
        [HttpGet]
        public IActionResult Wishlist()
        {
            return View();
        }
    }
}