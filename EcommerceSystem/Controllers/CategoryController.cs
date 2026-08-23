using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category (متاح للجميع: زوار، عملاء، وأدمن)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Include(c => c.SubCategories)
                .ToListAsync();

            return View(categories);
        }

        // GET: Category/Details/{id} (متاح للجميع)
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

        // GET: Category/Create (للأدمن فقط)
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

            if (!ModelState.IsValid)
                return View(category);

            if (categoryImage != null && categoryImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(categoryImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await categoryImage.CopyToAsync(fileStream);
                }

                category.CategoryImagePath = "/images/categories/" + uniqueFileName;
            }
            else
            {
                ModelState.AddModelError("", "Please select an image for the category.");
                return View(category);
            }

            category.CategoryId = Guid.NewGuid();
            category.IsDeleted = false;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/{id} (للأدمن فقط)
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

        // GET: Category/Delete/{id} (للأدمن فقط)
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