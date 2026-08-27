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
        public async Task<IActionResult> Index(string? search, int page = 1 )
        {
            int pageSize = 8;

            var query = _context.SubCategories
                .Include(sc => sc.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(sub => sub.SubCategoryName.Contains(term )
                                    || sub.Category.CategoryName.Contains(term) );
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, page);

            var subCategories = await query
                .OrderBy(s => s.SubCategoryName)
                .Skip((page -1 )* pageSize )
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TotalPages = totalPages;
            ViewBag.Page = page;
            ViewBag.TotalCount = totalCount;

            return View(subCategories);
        }

        // GET: SubCategory/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc =>
                    sc.SubCategoryId == id );

            if (subCategory == null)
                return NotFound();

            return View(subCategory);
        }

        // GET: SubCategory/Create
        public async Task<IActionResult> Create(Guid? categoryId)
        {
            ViewBag.Categories = new SelectList(await _context.Categories.Where(c => !c.IsDeleted).ToListAsync(), "CategoryId", "CategoryName", categoryId);

            var subCategory = new SubCategory();
            if (categoryId.HasValue)
            {
                subCategory.CategoryId = categoryId.Value;
            }

            return View(subCategory);
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

            subCategory.IsDeleted = true;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Subcategory deleted successfully.";

            return RedirectToAction(nameof(Index));
        }



        // GET: SubCategory/UnDelete/{id}
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnDelete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(c =>
                    c.SubCategoryId == id &&
                    c.IsDeleted);

            if (subCategory == null)
                return NotFound();

            return View("Delete", subCategory);
        }

        // POST: SubCategory/UnDelete/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("UnDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnDeleteConfirmed(Guid id)
        {
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(c =>
                    c.SubCategoryId == id &&
                    c.IsDeleted);

            if (subCategory == null)
                return NotFound();

            subCategory.IsDeleted = false;

            await _context.SaveChangesAsync();

            TempData["Success"] = "SubCategory restored successfully.";

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
