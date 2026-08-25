using EcommerceSystem.Data;
using EcommerceSystem.Helpers;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

        public async Task<IActionResult> Details(Guid? id, int? ratingFilter, string? customerSearch)
        {
            if (id == null)
                return NotFound();

            var productQuery = _context.Products
                .Where(p => !p.IsDeleted)
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductModel)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.ProductSubCategories)
                    .ThenInclude(psc => psc.SubCategory)
                .AsQueryable();

            var product = await productQuery.FirstOrDefaultAsync(p => p.ProductId == id.Value);

            if (product == null)
                return NotFound();

            bool hasPurchased = false;
            bool hasAlreadyReviewed = false;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
                    hasPurchased = customer.Orders
                        .SelectMany(o => o.OrderItems)
                        .Any(oi => oi.ProductId == product.ProductId);

                    hasAlreadyReviewed = product.ProductReviews
                        .Any(r => r.CustomerId == customer.CustomerId);
                }
            }

            ViewBag.HasPurchased = hasPurchased;
            ViewBag.HasAlreadyReviewed = hasAlreadyReviewed;

            // تصفية المراجعات
            var reviews = product.ProductReviews.AsEnumerable();

            if (ratingFilter.HasValue)
            {
                reviews = reviews.Where(r => r.CustomerProductRating == ratingFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(customerSearch))
            {
                reviews = reviews.Where(r => r.Customer != null &&
                    (r.Customer.FirstName + " " + r.Customer.LastName).Contains(customerSearch, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.FilteredReviews = reviews.ToList();
            ViewBag.SelectedRatingFilter = ratingFilter;
            ViewBag.CustomerSearch = customerSearch;

            if (product.ProductReviews.Any())
            {
                ViewBag.AverageRating = product.ProductReviews.Average(r => r.CustomerProductRating);
                ViewBag.TotalReviews = product.ProductReviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.TotalReviews = 0;
            }

            var userWishlistIds = new List<Guid>();
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Customer"))
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
                    userWishlistIds = await _context.WishlistItems
                        .Where(wi => wi.Wishlist != null && wi.Wishlist.CustomerId == customer.CustomerId)
                        .Select(wi => wi.ProductId)
                        .ToListAsync();
                }
            }
            ViewBag.UserWishlistIds = userWishlistIds;

            return View(product);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Guid productId, int rating, string reviewText)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile", "Customer");
            }

            bool hasPurchased = customer.Orders
                .SelectMany(o => o.OrderItems)
                .Any(oi => oi.ProductId == productId);

            if (!hasPurchased)
            {
                TempData["ErrorMessage"] = "You can only review products you have purchased.";
                return RedirectToAction("Details", new { id = productId });
            }

            bool existingReview = await _context.ProductReviews
                .AnyAsync(r => r.ProductId == productId && r.CustomerId == customer.CustomerId);

            if (existingReview)
            {
                TempData["ErrorMessage"] = "You have already reviewed this product.";
                return RedirectToAction("Details", new { id = productId });
            }

            if (ModelState.IsValid && rating >= 1 && rating <= 5)
            {
                var review = new ProductReview
                {
                    ProductReviewId = Guid.NewGuid(),
                    ProductId = productId,
                    CustomerId = customer.CustomerId,
                    CustomerProductRating = rating,
                    CustomerProductReview = reviewText ?? string.Empty,
                    CreatedAt = DateTime.Now
                };

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Review added successfully!";
            }

            return RedirectToAction("Details", new { id = productId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(Guid reviewId, Guid productId)
        {
            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Review deleted successfully.";
            }
            return RedirectToAction(nameof(Details), new { id = productId });
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
            List<Guid>? subCategoryIds,
            List<IFormFile>? imageFiles)
        {
            if (!ModelState.IsValid)
            {
                await LoadProductDropdowns();
                ViewBag.SelectedSubCategoryIds = subCategoryIds ?? new List<Guid>();
                return View(product);
            }

            product.ProductId = Guid.NewGuid();
            product.IsDeleted = false;

            foreach (var imageFile in imageFiles ?? new List<IFormFile>())
            {
                if (imageFile.Length == 0)
                    continue;

                var imagePath = await ImageUploadHelper.SaveImageAsync(imageFile, "products", _environment);

                product.ProductImages.Add(new ProductImage
                {
                    ProductImageId = Guid.NewGuid(),
                    ProductImagePath = imagePath,
                    ProductId = product.ProductId
                });
            }

            _context.Products.Add(product);

            if (subCategoryIds != null && subCategoryIds.Any())
            {
                foreach (var subId in subCategoryIds.Take(3))
                {
                    _context.ProductSubCategories.Add(new ProductSubCategory
                    {
                        ProductSubCategoryId = Guid.NewGuid(),
                        ProductId = product.ProductId,
                        SubCategoryId = subId
                    });
                }
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

            var selectedSubCategoryIds = product.ProductSubCategories
                        .Select(psc => psc.SubCategoryId)
                        .ToList();

            await LoadProductDropdowns();
            ViewBag.SelectedSubCategoryIds = selectedSubCategoryIds;

            return View(product);
        }

        // POST: Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            Product product,
            List<Guid>? subCategoryIds,
            List<IFormFile>? imageFiles,
            List<Guid>? deleteImageIds)
        {
            if (id != product.ProductId)
                return NotFound();

            var existingProduct = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSubCategories)
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    !p.IsDeleted);

            if (existingProduct == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadProductDropdowns();
                ViewBag.SelectedSubCategoryIds = subCategoryIds ?? new List<Guid>();
                return View(existingProduct);
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.ProductPrice = product.ProductPrice;
            existingProduct.ProductQuantity = product.ProductQuantity;
            existingProduct.ProductBrandId = product.ProductBrandId;
            existingProduct.ProductModelId = product.ProductModelId;

            if (deleteImageIds != null && deleteImageIds.Any())
            {
                foreach (var imgId in deleteImageIds)
                {
                    var imgRecord = await _context.ProductImages.FindAsync(imgId);
                    if (imgRecord != null)
                    {
                        _context.ProductImages.Remove(imgRecord);
                    }
                }
            }

            var uploadedFiles = (imageFiles ?? new List<IFormFile>())
                .Where(f => f.Length > 0)
                .ToList();

            foreach (var imageFile in uploadedFiles)
            {
                var imagePath = await ImageUploadHelper.SaveImageAsync(imageFile, "products", _environment);

                _context.ProductImages.Add(new ProductImage
                {
                    ProductImageId = Guid.NewGuid(),
                    ProductImagePath = imagePath,
                    ProductId = existingProduct.ProductId
                });
            }

            _context.ProductSubCategories.RemoveRange(
                existingProduct.ProductSubCategories);

            if (subCategoryIds != null && subCategoryIds.Any())
            {
                foreach (var subId in subCategoryIds.Take(3))
                {
                    _context.ProductSubCategories.Add(new ProductSubCategory
                    {
                        ProductSubCategoryId = Guid.NewGuid(),
                        ProductId = existingProduct.ProductId,
                        SubCategoryId = subId
                    });
                }
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

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            TempData["Warning"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadProductDropdowns(
            Guid? selectedSubCategoryId = null)
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
        }
    }
}