using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return View(categories);
        }

        // GET: Category/Details/{id}
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            category.CategoryId = Guid.NewGuid();
            category.IsDeleted = false;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/{id}
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(category);

            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id &&
                    !c.IsDeleted);

            if (existingCategory == null)
                return NotFound();

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryImagePath = category.CategoryImagePath;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Delete/{id}
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

            // Soft Delete
            category.IsDeleted = true;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Categories
                .Any(c => c.CategoryId == id && !c.IsDeleted);
        }
    }
}