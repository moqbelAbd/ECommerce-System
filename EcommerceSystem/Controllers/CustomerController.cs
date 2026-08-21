using EcommerceSystem.Data;
using EcommerceSystem.Models;
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
        // Browses all products, optionally filtered by category or subcategory.
        public async Task<IActionResult> Index(Guid? categoryId, Guid? subCategoryId)
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory)
                .AsQueryable();
            // Filter by SubCategory
            if (subCategoryId.HasValue)
            {
                products = products.Where(p => p.SubCategoryId == subCategoryId);
            }
            // Filter by Category
            else if (categoryId.HasValue)
            {
                products = products.Where(p => p.SubCategory != null && p.SubCategory.CategoryId == categoryId);
            }

            // Categories + active SubCategories

            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.subCategories.Where(sc => !sc.IsDeleted))
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;

            return View(await products.ToListAsync());
        }

        // GET: Customer/Search?term=...
        // Partial-text search across product description, brand, model, subcategory and category names.
        public async Task<IActionResult> Search(string? term)
        {
            var products = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc!.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                var search = term.Trim();

                products = products.Where(p =>
                    (p.ProductDescription != null && EF.Functions.Like(p.ProductDescription, $"%{search}%")) ||
                    (p.ProductBrand != null && EF.Functions.Like(p.ProductBrand.BrandName, $"%{search}%")) ||
                    (p.ProductModel != null && EF.Functions.Like(p.ProductModel.ModelName, $"%{search}%")) ||
                    (p.SubCategory != null && EF.Functions.Like(p.SubCategory.SubCategoryName, $"%{search}%")) ||
                    (p.SubCategory != null && p.SubCategory.Category != null && EF.Functions.Like(p.SubCategory.Category.CategoryName, $"%{search}%")));
            }

            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.subCategories.Where(sc => !sc.IsDeleted))
                .ToListAsync();

            ViewBag.SearchTerm = term;

            return View("Index", await products.ToListAsync());
        }

        // GET: Customer/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc!.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
                return NotFound();

            return View(product);
        }

        public IActionResult Cart()
        {
            return View();
        }
    }
}
