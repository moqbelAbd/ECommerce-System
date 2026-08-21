using EcommerceSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var products = _context.Products
                .Where(p => !p.IsDeleted)

                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                        .ThenInclude(sc => sc.Category)

                .AsQueryable();

            // Filter by SubCategory
            if (subCategoryId.HasValue)
            {
                products = products.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategoryId == subCategoryId.Value));
            }

            // Filter by Category
            else if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.CategoryId == categoryId.Value));
            }

            // Categories + active SubCategories
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)

                .Include(c => c.SubCategories
                    .Where(sc => !sc.IsDeleted))

                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;

            return View(await products.ToListAsync());
        }

        // GET: Customer/Search?term=...
        public async Task<IActionResult> Search(string? term)
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted)

                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                        .ThenInclude(sc => sc.Category)

                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim();

                products = products.Where(p =>

                    // Product description
                    (p.ProductDescription != null &&
                     EF.Functions.Like(
                         p.ProductDescription,
                         $"%{search}%"))

                    ||

                    // Brand
                    (p.ProductBrand != null &&
                     EF.Functions.Like(
                         p.ProductBrand.BrandName,
                         $"%{search}%"))

                    ||

                    // Model
                    (p.ProductModel != null &&
                     EF.Functions.Like(
                         p.ProductModel.ModelName,
                         $"%{search}%"))

                    ||

                    // SubCategory
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        EF.Functions.Like(
                            psc.SubCategory.SubCategoryName,
                            $"%{search}%"))

                    ||

                    // Category
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.Category != null &&
                        EF.Functions.Like(
                            psc.SubCategory.Category.CategoryName,
                            $"%{search}%"))
                );
            }

            // Categories + active SubCategories
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)

                .Include(c => c.SubCategories
                    .Where(sc => !sc.IsDeleted))

                .ToListAsync();

            ViewBag.SearchTerm = term;

            return View(
                "Index",
                await products.ToListAsync());
        }

        // GET: Customer/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products

                .Where(p => !p.IsDeleted)

                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                        .ThenInclude(sc => sc.Category)

                .FirstOrDefaultAsync(p =>
                    p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Customer/Cart
        [Authorize]
        public IActionResult Cart()
        {
            return View();
        }
    }
}