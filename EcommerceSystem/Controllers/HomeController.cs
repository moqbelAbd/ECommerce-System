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
                .Where(p => !p.IsDeleted && p.ProductQuantity >= 1)
                .Where(p => p.ProductSubCategories.Any(psc =>
                    psc.SubCategory != null &&
                    !psc.SubCategory.IsDeleted &&
                    psc.SubCategory.Category != null &&
                    !psc.SubCategory.Category.IsDeleted))
                .Include(p => p.ProductImages)
                .OrderBy(p => p.ProductName)
                .Take(8)
                .ToListAsync();

            var featuredCategories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName)
                .Take(10)
                .ToListAsync();

            var approvedTestimonials = await _context.Testimonials
                .Where(t => t.IsApproved)
                .Include(t => t.Customer)
                .OrderByDescending(t => t.TestimonialId)
                .Take(9)
                .ToListAsync();

            var userWishlistIds = new List<Guid>();
            bool hasSubmittedTestimonial = false;

            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
                    userWishlistIds = await _context.Wishlists
                        .Where(w => w.CustomerId == customer.CustomerId)
                        .SelectMany(w => w.WishlistItems)
                        .Select(wi => wi.ProductId)
                        .ToListAsync();

                    hasSubmittedTestimonial = await _context.Testimonials
                        .AnyAsync(t => t.CustomerId == customer.CustomerId);
                }
            }

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.FeaturedCategories = featuredCategories;
            ViewBag.Testimonials = approvedTestimonials;
            ViewBag.UserWishlistIds = userWishlistIds;
            ViewBag.HasSubmittedTestimonial = hasSubmittedTestimonial;

            return View(featuredProducts ?? new List<Product>());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTestimonial(string customerTestimonial)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile", "Customer");
            }

            bool existingTestimonial = await _context.Testimonials
                .AnyAsync(t => t.CustomerId == customer.CustomerId);

            if (existingTestimonial)
            {
                TempData["ErrorMessage"] = "You have already submitted a review for our store.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrWhiteSpace(customerTestimonial))
            {
                var testimonial = new Testimonial
                {
                    TestimonialId = Guid.NewGuid(),
                    CustomerTestimonial = customerTestimonial,
                    CustomerId = customer.CustomerId,
                    IsApproved = false 
                };

                _context.Testimonials.Add(testimonial);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Your testimonial has been submitted and is pending admin approval.";
            }

            return RedirectToAction(nameof(Index));
        }
        ActionResult Privacy()
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
    string? sort,
    int page = 1 )

        {
            int pageSize = 12;

            var query = _context.Products
      .Where(p => !p.IsDeleted && p.ProductQuantity >= 1)
      .Where(p => p.ProductSubCategories.Any(psc =>
          psc.SubCategory != null &&
          !psc.SubCategory.IsDeleted &&
          psc.SubCategory.Category != null &&
          !psc.SubCategory.Category.IsDeleted))
      .Include(p => p.ProductImages)
      .Include(p => p.ProductSubCategories)
          .ThenInclude(psc => psc.SubCategory)
      .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.CategoryId == categoryId.Value &&
                        !psc.SubCategory.IsDeleted &&
                        !psc.SubCategory.Category.IsDeleted));
            }

            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategoryId == subCategoryId.Value &&
                        !psc.SubCategory.IsDeleted &&
                        !psc.SubCategory.Category.IsDeleted));
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

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var userWishlistIds = new List<Guid>();
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
                    userWishlistIds = await _context.Wishlists
                .Where(w => w.CustomerId == customer.CustomerId)
                .SelectMany(w => w.WishlistItems)
                .Select(wi => wi.ProductId)
                .ToListAsync();
                            }
                        }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Sort = sort;
            ViewBag.UserWishlistIds = userWishlistIds;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.TotalCount = totalCount;

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