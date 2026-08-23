using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(
            Guid? categoryId,
            Guid? subCategoryId,
            Guid? brandId,
            Guid? modelId,
            decimal? minPrice,
            decimal? maxPrice,
            string? search)
        {
            var query = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.ProductName.Contains(search) ||
                    (p.ProductDescription != null &&
                     p.ProductDescription.Contains(search)) ||
                    (p.ProductBrand != null &&
                     p.ProductBrand.BrandName.Contains(search)) ||
                    (p.ProductModel != null &&
                     p.ProductModel.ModelName.Contains(search)));
            }

            // Category
            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategory != null &&
                        psc.SubCategory.CategoryId == categoryId.Value));
            }

            // SubCategory
            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategories.Any(psc =>
                        psc.SubCategoryId == subCategoryId.Value));
            }

            // Brand
            if (brandId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductBrandId == brandId.Value);
            }

            // Model
            if (modelId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductModelId == modelId.Value);
            }

            // Minimum Price
            if (minPrice.HasValue)
            {
                query = query.Where(p =>
                    p.ProductPrice >= minPrice.Value);
            }

            // Maximum Price
            if (maxPrice.HasValue)
            {
                query = query.Where(p =>
                    p.ProductPrice <= maxPrice.Value);
            }

            var products = await query
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            // Data for filters
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            ViewBag.SubCategories = await _context.SubCategories
                .Where(sc => !sc.IsDeleted)
                .ToListAsync();

            ViewBag.Brands = await _context.ProductBrands
                .ToListAsync();

            ViewBag.Models = await _context.ProductModels
                .ToListAsync();

            // Keep selected values
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSubCategoryId = subCategoryId;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SelectedModelId = modelId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Search = search;

            return View(products);
        }

        // GET: Product/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)

                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            await LoadProductDropdowns();

            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            Guid? subCategoryId,
            string? imagePaths)
        {
            if (!ModelState.IsValid)
            {
                await LoadProductDropdowns(subCategoryId, imagePaths);
                return View(product);
            }

            product.ProductId = Guid.NewGuid();
            product.IsDeleted = false;

            // Add product images
            foreach (var path in SplitImagePaths(imagePaths))
            {
                product.ProductImages.Add(new ProductImage
                {
                    ProductImageId = Guid.NewGuid(),
                    ProductImagePath = path,
                    ProductId = product.ProductId
                });
            }

            _context.Products.Add(product);

            // Add Product <-> SubCategory relationship
            if (subCategoryId.HasValue)
            {
                var productSubCategory = new ProductSubCategory
                {
                    ProductSubCategoryId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    SubCategoryId = subCategoryId.Value
                };

                _context.ProductSubCategories.Add(productSubCategory);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSubCategories)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (product == null)
                return NotFound();

            var selectedSubCategoryId = product.ProductSubCategories
                .Select(psc => (Guid?)psc.SubCategoryId)
                .FirstOrDefault();

            var imagePaths = string.Join(", ", product.ProductImages
                .Select(image => image.ProductImagePath));

            await LoadProductDropdowns(selectedSubCategoryId, imagePaths);

            return View(product);
        }

        // POST: Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            Product product,
            Guid? subCategoryId,
            string? imagePaths)
        {
            if (id != product.ProductId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadProductDropdowns(subCategoryId, imagePaths);
                return View(product);
            }

            var existingProduct = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSubCategories)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (existingProduct == null)
                return NotFound();

            // Update product information
            existingProduct.ProductName =
                product.ProductName;

            existingProduct.ProductDescription =
                product.ProductDescription;

            existingProduct.ProductPrice =
                product.ProductPrice;

            existingProduct.ProductQuantity =
                product.ProductQuantity;

            existingProduct.ProductBrandId =
                product.ProductBrandId;

            existingProduct.ProductModelId =
                product.ProductModelId;

            // Remove old images
            _context.ProductImages.RemoveRange(
                existingProduct.ProductImages);

            // Add new images
            foreach (var path in SplitImagePaths(imagePaths))
            {
                existingProduct.ProductImages.Add(new ProductImage
                {
                    ProductImageId = Guid.NewGuid(),
                    ProductImagePath = path,
                    ProductId = existingProduct.ProductId
                });
            }

            // Remove old Product/SubCategory relationships
            _context.ProductSubCategories.RemoveRange(
                existingProduct.ProductSubCategories);

            // Add new relationship
            if (subCategoryId.HasValue)
            {
                existingProduct.ProductSubCategories.Add(
                    new ProductSubCategory
                    {
                        ProductSubCategoryId = Guid.NewGuid(),
                        ProductId = existingProduct.ProductId,
                        SubCategoryId = subCategoryId.Value
                    });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products

                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)

                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)

                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Product/Delete/{id}
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (product == null)
                return NotFound();

            // Soft Delete
            product.IsDeleted = true;

            await _context.SaveChangesAsync();

            TempData["Warning"] = "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Load dropdown data
        private async Task LoadProductDropdowns(
            Guid? selectedSubCategoryId = null,
            string? imagePaths = null)
        {
            ViewBag.Brands = await _context.ProductBrands
                .OrderBy(brand => brand.BrandName)
                .ToListAsync();

            ViewBag.Models = await _context.ProductModels
                .OrderBy(model => model.ModelName)
                .ToListAsync();

            ViewBag.SubCategories = await _context.SubCategories
                .Where(sc => !sc.IsDeleted)
                .OrderBy(subCategory => subCategory.SubCategoryName)
                .ToListAsync();

            ViewBag.SelectedSubCategoryId = selectedSubCategoryId;
            ViewBag.ImagePaths = imagePaths;
        }

        // Split image paths
        private static IEnumerable<string> SplitImagePaths(
            string? imagePaths)
        {
            if (string.IsNullOrWhiteSpace(imagePaths))
                yield break;

            foreach (var path in imagePaths.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries))
            {
                yield return path;
            }
        }
    }
}
