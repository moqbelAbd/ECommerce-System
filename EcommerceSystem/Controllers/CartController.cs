using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EcommerceSystem.Controllers
{
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            if (User.Identity != null && User.Identity.IsAuthenticated )
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer == null) return Unauthorized();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                if (cart == null)
                {
                    cart = new Cart { CustomerId = customer.CustomerId };
                    _context.Add(cart);
                }

                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                int currentCartQty = existingItem != null ? existingItem.ItemQuantity : 0;

                if (currentCartQty + quantity > product.ProductQuantity) 
                {
                    return Json(new { success = false, message = $"Cannot add item. Only {product.ProductQuantity} available in stock." });
                }

                if (existingItem == null)
                {
                    cart.CartItems.Add(new CartItem { ProductId = productId, ItemQuantity = quantity });
                }
                else
                {
                    existingItem.ItemQuantity += quantity;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Added to your cart" });
            }

            else
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                var cartList = string.IsNullOrEmpty(sessionCartStr)
                    ? new List<SessionCartItem>()
                    : JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);

                var existingItem = cartList!.FirstOrDefault(i => i.ProductId == productId);
                int currentCartQty = existingItem != null ? existingItem.Quantity : 0;

                if (currentCartQty + quantity > product.ProductQuantity)
                {
                    return Json(new { success = false, message = $"Cannot add item. Only {product.ProductQuantity} available in stock." });
                }

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cartList.Add(new SessionCartItem { ProductId = productId, Quantity = quantity });
                }

                HttpContext.Session.SetString("GuestCart", JsonSerializer.Serialize(cartList));
                return Json(new { success = true, type = "Session", message = "Added to guest cart!" });
            }
        }


        [HttpGet]
        public IActionResult RefreshSidebarCart()
        {
            // This  re-runs CartSidebarViewComponent and returns just the HTML!
            return ViewComponent("CartSidebar");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid productId)
        {
            // 1. Customer Logic
            if (User.Identity != null && User.Identity.IsAuthenticated )
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
                if (customer != null)
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                    if (cart != null)
                    {
                        var itemToRemove = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                        if (itemToRemove != null)
                        {
                            _context.CartItems.Remove(itemToRemove);
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                // تجاهل الخطأ في حال كانت العنصر محذوف مسبقاً لتجنب توقف البرنامج
                                _context.Entry(itemToRemove).State = EntityState.Detached;
                            }
                        }
                    }
                }
            }
            // 2. Guest Logic (Session)
            else
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(sessionCartStr))
                {
                    var cartList = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
                    var itemToRemove = cartList!.FirstOrDefault(i => i.ProductId == productId);
                    if (itemToRemove != null)
                    {
                        cartList!.Remove(itemToRemove);
                        HttpContext.Session.SetString("GuestCart", JsonSerializer.Serialize(cartList));
                    }
                }
            }

            // إرجاع الـ JSON المطلوب للـ AJAX والـ Toast
            return Json(new { success = true, message = "Product removed from cart successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<Guid, int> quantities)
        {
            // 1. Customer Logic
            if (User.Identity != null && User.Identity.IsAuthenticated )
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
                var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.CustomerId == customer!.CustomerId);

                if (cart != null)
                {
                    foreach (var kvp in quantities)
                    {
                        var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == kvp.Key);
                        if (item != null)
                        {
                            // Optional: You can check product stock limits again right here!
                            item.ItemQuantity = kvp.Value > 0 ? kvp.Value : 1;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            // 2. Guest Logic (Session)
            else
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(sessionCartStr))
                {
                    var cartList = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
                    foreach (var kvp in quantities)
                    {
                        var item = cartList!.FirstOrDefault(i => i.ProductId == kvp.Key);
                        if (item != null)
                        {
                            item.Quantity = kvp.Value > 0 ? kvp.Value : 1;
                        }
                    }
                    HttpContext.Session.SetString("GuestCart", JsonSerializer.Serialize(cartList));
                }
            }

            return RedirectToAction("Cart", "Customer");
        }

    }
}
