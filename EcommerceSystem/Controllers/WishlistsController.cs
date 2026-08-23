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

        // GET: Wishlists/MyWishlist
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyWishlist()
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

            return View(wishlist);
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
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // البحث عن قائمة الأمنيات أو إنشاء واحدة جديدة إن لم تكن موجودة
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
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

            // التحقق هل المنتج موجود في قائمة الأمنيات مسبقاً
            var existingItem = wishlist.WishlistItems
                .FirstOrDefault(wi => wi.ProductId == productId);

            if (existingItem != null)
            {
                // إزالة المنتج إذا كان موجوداً
                _context.WishlistItems.Remove(existingItem);
                TempData["Success"] = "Product removed from your wishlist.";
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
                TempData["Success"] = "Product added to your wishlist!";
            }

            await _context.SaveChangesAsync();

            // العودة إلى نفس الصفحة التي جاء منها العميل
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index", "Home"));
        }


        // =========================================================
        // ADMIN ACTIONS (Manage All Wishlists)
        // =========================================================

        // GET: WISHLISTS
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var wishlists = await _context.Wishlists
                .Include(w => w.Customer)
                .Include(w => w.WishlistItems)
                .ToListAsync();

            return View(wishlists);
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

        // GET: WISHLISTS/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: WISHLISTS/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WishlistId,CustomerId")] Wishlist wishlist)
        {
            if (ModelState.IsValid)
            {
                wishlist.WishlistId = Guid.NewGuid();
                _context.Add(wishlist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wishlist);
        }

        // GET: WISHLISTS/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? wishlistid)
        {
            if (wishlistid == null)
            {
                return NotFound();
            }

            var wishlist = await _context.Wishlists.FindAsync(wishlistid);
            if (wishlist == null)
            {
                return NotFound();
            }
            return View(wishlist);
        }

        // POST: WISHLISTS/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid wishlistid, [Bind("WishlistId,CustomerId")] Wishlist wishlist)
        {
            if (wishlistid != wishlist.WishlistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wishlist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WishlistExists(wishlist.WishlistId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        private bool WishlistExists(Guid wishlistid)
        {
            return _context.Wishlists.Any(e => e.WishlistId == wishlistid);
        }
    }
}