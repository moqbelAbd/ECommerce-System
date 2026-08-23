using EcommerceSystem.Data;
using EcommerceSystem.Helpers;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SubCategoryController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: SubCategory
        public async Task<IActionResult> Index()
        {
            var subCategories = await _context.SubCategories
                .Where(sc => !sc.IsDeleted)
                .Include(sc => sc.Category)
                .ToListAsync();

            return View(subCategories);
        }

        // GET: SubCategory/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id &&
                    !sc.IsDeleted);

            if (subCategory == null)
                return NotFound();

            return View(subCategory);
        }

        // GET: SubCategory/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View();
        }

        // POST: SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategory subCategory, IFormFile? imageFile)
        {
            ModelState.Remove(nameof(SubCategory.SubCategoryImagePath));

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(SubCategory.SubCategoryImagePath), "Please choose an image to upload.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(subCategory.CategoryId);
                return View(subCategory);
            }

            subCategory.SubCategoryId = Guid.NewGuid();
            subCategory.IsDeleted = false;
            subCategory.SubCategoryImagePath = await ImageUploadHelper.SaveImageAsync(imageFile!, "subcategories", _environment);

            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Subcategory created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: SubCategory/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id &&
                    !sc.IsDeleted);

            if (subCategory == null)
                return NotFound();

            await PopulateCategoriesAsync(subCategory.CategoryId);
            return View(subCategory);
        }

        // POST: SubCategory/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SubCategory subCategory, IFormFile? imageFile)
        {
            if (id != subCategory.SubCategoryId)
                return NotFound();

            ModelState.Remove(nameof(SubCategory.SubCategoryImagePath));

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(subCategory.CategoryId);
                return View(subCategory);
            }

            var existingSubCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id &&
                    !sc.IsDeleted);

            if (existingSubCategory == null)
                return NotFound();

            existingSubCategory.SubCategoryName = subCategory.SubCategoryName;
            existingSubCategory.CategoryId = subCategory.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                existingSubCategory.SubCategoryImagePath = await ImageUploadHelper.SaveImageAsync(imageFile, "subcategories", _environment);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Subcategory updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: SubCategory/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id &&
                    !sc.IsDeleted);

            if (subCategory == null)
                return NotFound();

            return View(subCategory);
        }

        // POST: SubCategory/Delete/{id}
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id &&
                    !sc.IsDeleted);

            if (subCategory == null)
                return NotFound();

            // Soft Delete
            subCategory.IsDeleted = true;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Subcategory deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategoriesAsync(Guid? selectedCategoryId = null)
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", selectedCategoryId);
        }
    }
}
