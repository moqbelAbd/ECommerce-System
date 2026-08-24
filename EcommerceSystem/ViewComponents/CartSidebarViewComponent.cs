using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EcommerceSystem.ViewComponents
{
    public class CartSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartSidebarViewComponent(ApplicationDbContext contxt)
        {
            _context = contxt;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartViewModel = new CartViewModel();

            // SCENARIO A: Logged-in Customer
            if (UserClaimsPrincipal != null && UserClaimsPrincipal.Identity != null && UserClaimsPrincipal.Identity.IsAuthenticated)
            {
                var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

                if (customer != null)
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Product)
                                .ThenInclude(p => p.ProductImages)
                        .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

                    if (cart != null)
                    {
                        cartViewModel.Items = cart.CartItems.Select(ci => new CartItemViewModel
                        {
                            ProductId = ci.ProductId,
                            ProductName = ci.Product!.ProductName,

                            // FIX: Safely handles products with no images
                            ImageUrl = ci.Product.ProductImages.FirstOrDefault().ProductImagePath,

                            Price = ci.Product.ProductPrice,
                            Quantity = ci.ItemQuantity
                        }).ToList();
                    }
                }
            }
            // SCENARIO B: Guest (Session Cart)
            else
            {
                var sessionCartStr = HttpContext.Session.GetString("GuestCart");
                if (!string.IsNullOrEmpty(sessionCartStr))
                {
                    var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);

                    if (sessionItems != null)
                    {
                        foreach (var item in sessionItems)
                        {
                            // Fetch product with images for the guest cart too!
                            var product = await _context.Products
                                .Include(p => p.ProductImages)
                                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                            if (product != null)
                            {
                                cartViewModel.Items.Add(new CartItemViewModel
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName,
                                    ImageUrl = product.ProductImages.FirstOrDefault().ProductImagePath ,
                                    Price = product.ProductPrice,
                                    Quantity = item.Quantity
                                });
                            }
                        }
                    }
                }
            }

            return View(cartViewModel);
        }
    }
}