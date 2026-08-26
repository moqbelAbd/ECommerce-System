// Licensed to the .NET Foundation under one or more agreements.// The .NET Foundation licenses this file to you under the MIT license.using System;
using EcommerceSystem.Data;
using EcommerceSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace EcommerceSystem.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager; // أضفنا مدير المستخدمين هنا
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager; // حقنه في الـ Constructor
        _logger = logger;
        _context = context;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    [HttpPost]
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/"); // الرابط الافتراضي إذا لم يوجد returnUrl

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // هنا يتم إعادة توجيه المستخدم للصفحة الأصلية التي كان يحاول الوصول لها!
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // إذا حدث خطأ، نبقي الـ returnUrl موجوداً لكي لا يضيع
        return Page();
    }
    private async Task MergeSessionCartToDatabaseAsync(string applicationUserId)
    {
        // 1. Check if there is anything in the guest cart
        var sessionCartStr = HttpContext.Session.GetString("GuestCart");
        if (string.IsNullOrEmpty(sessionCartStr)) return;

        var sessionItems = JsonSerializer.Deserialize<List<SessionCartItem>>(sessionCartStr);
        if (sessionItems == null || !sessionItems.Any()) return;

        // 2. Find the logged-in Customer record
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
        if (customer == null) return;

        // 3. Find or Create their Database Cart
        var dbCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

        if (dbCart == null)
        {
            dbCart = new Cart { CustomerId = customer.CustomerId };
            _context.Carts.Add(dbCart);
        }

        // 4. Merge the items
        foreach (var sessionItem in sessionItems)
        {
            var existingDbItem = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId);
            if (existingDbItem != null)
            {
                // Add session quantity to existing database quantity
                existingDbItem.ItemQuantity += sessionItem.Quantity;
            }
            else
            {
                // Add brand new item to database cart
                dbCart.CartItems.Add(new CartItem
                {
                    ProductId = sessionItem.ProductId,
                    ItemQuantity = sessionItem.Quantity
                });
            }
        }

        // 5. Save changes and destroy the temporary session cart
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("GuestCart");
    }
}