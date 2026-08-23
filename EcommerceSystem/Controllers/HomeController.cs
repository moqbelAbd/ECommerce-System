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

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductImages)
                .OrderBy(p => p.ProductName)
                .Take(8)
                .ToListAsync();

            var featuredCategories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName)
                .Take(3)
                .ToListAsync();

            ViewBag.FeaturedCategories = featuredCategories;

            // تمرير المنتجات مباشرة إلى الـ View عبر الـ Model
            return View(featuredProducts ?? new List<Product>());
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

        public async Task<IActionResult> Shop(
            Guid? categoryId,
            Guid? subCategoryId,
            string? searchTerm,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted)
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
                var term = searchTerm.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(term) ||
                    (p.ProductDescription != null && p.ProductDescription.Contains(term)));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.ProductPrice),
                "price_desc" => query.OrderByDescending(p => p.ProductPrice),
                "name_desc" => query.OrderByDescending(p => p.ProductName),
                _ => query.OrderBy(p => p.ProductName)
            };

            var products = await query.ToListAsync();

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;

            return View(products);
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