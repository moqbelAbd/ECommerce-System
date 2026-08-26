using EcommerceSystem.Data;
using EcommerceSystem.Helpers;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Category 
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Admin"))
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.SubCategories)
                    .ToListAsync();

                return View(categories);

            }
            else
            {
                var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();

                return View(categories);
            }
        }

        // GET: Category/Details/{id} 
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: Category/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile? categoryImage)
        {
            ModelState.Remove(nameof(Category.CategoryImagePath));

            if (categoryImage == null || categoryImage.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an image for the category.");
            }

            if (!ModelState.IsValid)
                return View(category);

            category.CategoryId = Guid.NewGuid();
            category.IsDeleted = false;
            category.CategoryImagePath = await ImageUploadHelper.SaveImageAsync(categoryImage!, "categories", _environment);

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Edit/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category, IFormFile? categoryImage)
        {
            if (id != category.CategoryId)
                return NotFound();

            ModelState.Remove(nameof(Category.CategoryImagePath));

            if (!ModelState.IsValid)
                return View(category);

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (existingCategory == null)
                return NotFound();

            existingCategory.CategoryName = category.CategoryName;

            if (categoryImage != null && categoryImage.Length > 0)
            {
                existingCategory.CategoryImagePath = await ImageUploadHelper.SaveImageAsync(categoryImage, "categories", _environment);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Category/Delete/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (category == null)
                return NotFound();

            category.IsDeleted = true;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }



        // GET: Category/UnDelete/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnDelete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    c.IsDeleted);

            if (category == null)
                return NotFound();

            return View("Delete", category);
        }

        // POST: Category/UnDelete/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("UnDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnDeleteConfirmed(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    c.IsDeleted);

            if (category == null)
                return NotFound();

            category.IsDeleted = false;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Category restored successfully.";

            return RedirectToAction(nameof(Index));
        }
        private bool CategoryExists(Guid id)
        {
            return _context.Categories
                .Any(c => c.CategoryId == id && !c.IsDeleted);
        }
    }
}
