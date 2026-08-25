using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceSystem.Models;
using EcommerceSystem.Data;
using System.Security.Claims;

namespace EcommerceSystem.Controllers
{
    public class WishlistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CUSTOMER ACTIONS (Only Logged-in Customers)
        // =========================================================

        // GET: Wishlists/Index (عرض قائمة الأمنيات الخاصة بالعميل)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return RedirectToAction("CompleteProfile", "Customer");
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(w => w.CustomerId == customer.CustomerId);

            // تحديد مسار الـ View بالاسم الصحيح لمجلد Wishlist (مفرد)
            return View("~/Views/Wishlist/Index.cshtml", wishlist);
        }

        // POST: Wishlists/ToggleWishlist
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleWishlist(Guid productId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
            {
                return Json(new { success = false, redirect = Url.Page("/Account/Login", new { area = "Identity" }) });
            }

            // 1. البحث عن قائمة الأمنيات الخاصة بالعميل أو إنشاؤها إن لم تكن موجودة
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.CustomerId == customer.CustomerId);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    WishlistId = Guid.NewGuid(),
                    CustomerId = customer.CustomerId
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            // 2. التحقق المباشر من وجود المنتج في جدول العناصر الوسيطة
            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.WishlistId && wi.ProductId == productId);

            bool isAdded = false;

            if (existingItem != null)
            {
                // إزالة المنتج إذا كان موجوداً
                _context.WishlistItems.Remove(existingItem);
                isAdded = false;
            }
            else
            {
                // إضافة المنتج إذا لم يكن موجوداً
                var wishlistItem = new WishlistItem
                {
                    WishlistItemId = Guid.NewGuid(),
                    WishlistId = wishlist.WishlistId,
                    ProductId = productId
                };
                _context.WishlistItems.Add(wishlistItem);
                isAdded = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // في حال حدوث تداخل، نقوم بإلغاء التتبع وإعادة المحاولة أو تجاهله بأمان
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.Reload();
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, isAdded = isAdded });
        }


        // =========================================================
        // ADMIN ACTIONS (Manage All Wishlists)
        // =========================================================

        // GET: Wishlists/AdminList
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var wishlists = await _context.Wishlists
                .Include(w => w.Customer)
                .Include(w => w.WishlistItems)
                .ToListAsync();

            return View("AdminIndex", wishlists);
        }

        // GET: WISHLISTS/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? wishlistid)
        {
            if (wishlistid == null)
            {
                return NotFound();
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Customer)
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(m => m.WishlistId == wishlistid);

            if (wishlist == null)
            {
                return NotFound();
            }

            return View(wishlist);
        }

        // GET: WISHLISTS/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? wishlistid)
        {
            if (wishlistid == null)
            {
                return NotFound();
            }

            var wishlist = await _context.Wishlists
                .Include(w => w.Customer)
                .FirstOrDefaultAsync(m => m.WishlistId == wishlistid);

            if (wishlist == null)
            {
                return NotFound();
            }

            return View(wishlist);
        }

        // POST: WISHLISTS/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid wishlistid)
        {
            var wishlist = await _context.Wishlists.FindAsync(wishlistid);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdminList));
        }

        private bool WishlistExists(Guid wishlistid)
        {
            return _context.Wishlists.Any(e => e.WishlistId == wishlistid);
        }
    }
}