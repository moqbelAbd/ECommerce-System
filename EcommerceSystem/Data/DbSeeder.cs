using EcommerceSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceSystem.Data
{
    public static class DbSeeder
    {
        // Fixed marker email so re-running the app never seeds this demo data twice.
        private const string MarkerCustomerEmail = "sophia.demo@seedmail.com";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByEmailAsync(MarkerCustomerEmail) != null)
            {
                return; // Already seeded.
            }

            // ---------- Categories ----------
            var luxuryCategoryId = Guid.Parse("c1111111-1111-1111-1111-111111111111");
            var sportCategoryId = Guid.Parse("c2222222-2222-2222-2222-222222222222");
            var smartCategoryId = Guid.Parse("c3333333-3333-3333-3333-333333333333");

            var categories = new List<Category>
            {
                new() { CategoryId = luxuryCategoryId, CategoryName = "Luxury Watches", CategoryImagePath = "/images/categories/0eba9dd8-c512-424b-bd2b-e5e3a14cd0d8.jpg" },
                new() { CategoryId = sportCategoryId, CategoryName = "Sport Watches", CategoryImagePath = "/images/categories/281ff08e-da8c-4cb5-addd-a18f46874e43.jpg" },
                new() { CategoryId = smartCategoryId, CategoryName = "Smart Watches", CategoryImagePath = "/images/categories/3d36146c-a410-4ce3-b12c-98a42550ef1e.jpeg" },
            };
            await context.Categories.AddRangeAsync(categories);

            // ---------- Sub Categories ----------
            var menLuxuryId = Guid.Parse("d1111111-1111-1111-1111-111111111111");
            var womenLuxuryId = Guid.Parse("d1111111-2222-2222-2222-222222222222");
            var runningId = Guid.Parse("d2222222-1111-1111-1111-111111111111");
            var divingId = Guid.Parse("d2222222-2222-2222-2222-222222222222");
            var fitnessId = Guid.Parse("d3333333-1111-1111-1111-111111111111");
            var kidsId = Guid.Parse("d3333333-2222-2222-2222-222222222222");

            var subCategories = new List<SubCategory>
            {
                new() { SubCategoryId = menLuxuryId, SubCategoryName = "Men's Luxury", SubCategoryImagePath = "/images/subcategories/0ffded23-b118-4c28-9364-a8afd2cc8dd0.png", CategoryId = luxuryCategoryId },
                new() { SubCategoryId = womenLuxuryId, SubCategoryName = "Women's Luxury", SubCategoryImagePath = "/images/subcategories/1cf43a56-2f33-4e26-9a2d-d85228dafdde.jpg", CategoryId = luxuryCategoryId },
                new() { SubCategoryId = runningId, SubCategoryName = "Running", SubCategoryImagePath = "/images/subcategories/3aba9449-f9d7-4c87-bcc0-872fbe4cb29c.png", CategoryId = sportCategoryId },
                new() { SubCategoryId = divingId, SubCategoryName = "Diving", SubCategoryImagePath = "/images/subcategories/42c1285a-d965-4cb4-889a-1b6ac4e74a0c.png", CategoryId = sportCategoryId },
                new() { SubCategoryId = fitnessId, SubCategoryName = "Fitness Tracking", SubCategoryImagePath = "/images/subcategories/46bddd56-bc41-466e-a6c0-65d11208847e.jpg", CategoryId = smartCategoryId },
                new() { SubCategoryId = kidsId, SubCategoryName = "Kids", SubCategoryImagePath = "/images/subcategories/522badc5-0e8d-43ba-b862-9a02bddc28b6.png", CategoryId = smartCategoryId },
            };
            await context.SubCategories.AddRangeAsync(subCategories);

            // ---------- Products ----------
            // Brand/Model ids come from the lookup data already seeded in ApplicationDbContext.OnModelCreating.
            var rolexSubmariner = Guid.Parse("e1111111-1111-1111-1111-111111111111");
            var rolexDatejust = Guid.Parse("e1111111-2222-2222-2222-222222222222");
            var tissotLeLocle = Guid.Parse("e2222222-1111-1111-1111-111111111111");
            var tissotPrx = Guid.Parse("e2222222-2222-2222-2222-222222222222");
            var casioGShock = Guid.Parse("e3333333-1111-1111-1111-111111111111");
            var casioEdifice = Guid.Parse("e3333333-2222-2222-2222-222222222222");
            var appleSeries9 = Guid.Parse("e4444444-1111-1111-1111-111111111111");
            var appleUltra2 = Guid.Parse("e4444444-2222-2222-2222-222222222222");

            var rolexBrand = Guid.Parse("b1111111-1111-1111-1111-111111111111");
            var tissotBrand = Guid.Parse("b2222222-2222-2222-2222-222222222222");
            var casioBrand = Guid.Parse("b3333333-3333-3333-3333-333333333333");
            var appleBrand = Guid.Parse("b4444444-4444-4444-4444-444444444444");

            var products = new List<Product>
            {
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000001"), ProductName = "Rolex Submariner Date", ProductDescription = "Iconic diver's watch with date function.", ProductPrice = 9500m, ProductQuantity = 4, ProductBrandId = rolexBrand, ProductModelId = rolexSubmariner },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000002"), ProductName = "Rolex Datejust 41", ProductDescription = "Timeless classic with fluted bezel.", ProductPrice = 8700m, ProductQuantity = 3, ProductBrandId = rolexBrand, ProductModelId = rolexDatejust },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000003"), ProductName = "Tissot Le Locle Automatic", ProductDescription = "Elegant automatic dress watch.", ProductPrice = 650m, ProductQuantity = 15, ProductBrandId = tissotBrand, ProductModelId = tissotLeLocle },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000004"), ProductName = "Tissot PRX Powermatic", ProductDescription = "Retro-inspired steel bracelet watch.", ProductPrice = 800m, ProductQuantity = 10, ProductBrandId = tissotBrand, ProductModelId = tissotPrx },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000005"), ProductName = "Casio G-Shock Rangeman", ProductDescription = "Rugged tool watch built for the outdoors.", ProductPrice = 320m, ProductQuantity = 25, ProductBrandId = casioBrand, ProductModelId = casioGShock },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000006"), ProductName = "Casio Edifice Chronograph", ProductDescription = "Sport chronograph with sapphire glass.", ProductPrice = 180m, ProductQuantity = 30, ProductBrandId = casioBrand, ProductModelId = casioEdifice },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000007"), ProductName = "Apple Watch Series 9", ProductDescription = "Smartwatch with health tracking.", ProductPrice = 429m, ProductQuantity = 20, ProductBrandId = appleBrand, ProductModelId = appleSeries9 },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000008"), ProductName = "Apple Watch Ultra 2", ProductDescription = "Rugged titanium smartwatch for athletes.", ProductPrice = 799m, ProductQuantity = 12, ProductBrandId = appleBrand, ProductModelId = appleUltra2 },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-000000000009"), ProductName = "Casio G-Shock Mudmaster", ProductDescription = "Shock and mud resistant field watch.", ProductPrice = 260m, ProductQuantity = 4, ProductBrandId = casioBrand, ProductModelId = casioGShock },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-00000000000a"), ProductName = "Tissot Seastar Diver", ProductDescription = "300m water resistant dive watch.", ProductPrice = 720m, ProductQuantity = 3, ProductBrandId = tissotBrand, ProductModelId = tissotLeLocle },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-00000000000b"), ProductName = "Rolex Explorer II", ProductDescription = "Adventure-ready GMT watch.", ProductPrice = 9900m, ProductQuantity = 2, ProductBrandId = rolexBrand, ProductModelId = rolexDatejust },
                new() { ProductId = Guid.Parse("f0000001-0000-0000-0000-00000000000c"), ProductName = "Apple Watch SE", ProductDescription = "Affordable smartwatch for everyday use.", ProductPrice = 249m, ProductQuantity = 18, ProductBrandId = appleBrand, ProductModelId = appleSeries9 },
            };
            await context.Products.AddRangeAsync(products);

            var productImages = products.Select(p => new ProductImage
            {
                ProductImageId = Guid.NewGuid(),
                ProductImagePath = "/images/products/default-product.jpg",
                ProductId = p.ProductId
            }).ToList();
            await context.ProductImages.AddRangeAsync(productImages);

            var subCategoryByProduct = new (Guid ProductId, Guid SubCategoryId)[]
            {
                (products[0].ProductId, menLuxuryId),
                (products[1].ProductId, womenLuxuryId),
                (products[2].ProductId, menLuxuryId),
                (products[3].ProductId, womenLuxuryId),
                (products[4].ProductId, divingId),
                (products[5].ProductId, runningId),
                (products[6].ProductId, fitnessId),
                (products[7].ProductId, fitnessId),
                (products[8].ProductId, divingId),
                (products[9].ProductId, divingId),
                (products[10].ProductId, menLuxuryId),
                (products[11].ProductId, kidsId),
            };
            await context.ProductSubCategories.AddRangeAsync(subCategoryByProduct.Select(x => new ProductSubCategory
            {
                ProductSubCategoryId = Guid.NewGuid(),
                ProductId = x.ProductId,
                SubCategoryId = x.SubCategoryId
            }));

            // ---------- Customers (Identity users + profile) ----------
            var demoCustomers = new[]
            {
                new { Email = MarkerCustomerEmail, First = "Sophia", Last = "Miller", Location = "Amman, Jordan", Phone = "0790000001" },
                new { Email = "liam.demo@seedmail.com", First = "Liam", Last = "Johnson", Location = "Irbid, Jordan", Phone = "0790000002" },
                new { Email = "olivia.demo@seedmail.com", First = "Olivia", Last = "Brown", Location = "Zarqa, Jordan", Phone = "0790000003" },
                new { Email = "noah.demo@seedmail.com", First = "Noah", Last = "Davis", Location = "Aqaba, Jordan", Phone = "0790000004" },
                new { Email = "emma.demo@seedmail.com", First = "Emma", Last = "Wilson", Location = "Salt, Jordan", Phone = "0790000005" },
                new { Email = "james.demo@seedmail.com", First = "James", Last = "Taylor", Location = "Madaba, Jordan", Phone = "0790000006" },
                new { Email = "ava.demo@seedmail.com", First = "Ava", Last = "Anderson", Location = "Karak, Jordan", Phone = "0790000007" },
            };

            const string demoPassword = "Seed@12345";
            var customers = new List<Customer>();

            foreach (var d in demoCustomers)
            {
                var appUser = new ApplicationUser { UserName = d.Email, Email = d.Email, EmailConfirmed = true };
                var createResult = await userManager.CreateAsync(appUser, demoPassword);
                if (!createResult.Succeeded)
                {
                    continue;
                }

                await userManager.AddToRoleAsync(appUser, "Customer");

                var customer = new Customer
                {
                    CustomerId = Guid.NewGuid(),
                    FirstName = d.First,
                    LastName = d.Last,
                    Location = d.Location,
                    ApplicationUserId = appUser.Id
                };
                customers.Add(customer);
                await context.Customers.AddAsync(customer);

                await context.CustomerPhoneNumbers.AddAsync(new CustomerPhoneNumber
                {
                    PhoneNumberId = Guid.NewGuid(),
                    PhoneNumber = d.Phone,
                    CustomerId = customer.CustomerId
                });
            }

            await context.SaveChangesAsync();

            // ---------- Orders ----------
            var orderStatusIds = new[] { 1, 2, 3, 4 };
            var paymentStatusIds = new[] { 1, 2 };
            var rng = new Random(12345);

            foreach (var customer in customers)
            {
                var orderCount = rng.Next(1, 3); // 1-2 orders per customer

                for (int i = 0; i < orderCount; i++)
                {
                    var pickedProducts = products.OrderBy(_ => rng.Next()).Take(rng.Next(1, 3)).ToList();
                    var order = new Order
                    {
                        OrderId = Guid.NewGuid(),
                        CreatedAt = DateTime.Now.AddDays(-rng.Next(1, 60)),
                        OrderStatusId = orderStatusIds[rng.Next(orderStatusIds.Length)],
                        PaymentStatusId = paymentStatusIds[rng.Next(paymentStatusIds.Length)],
                        PaymentTypeId = rng.Next(1, 4),
                        Location = customer.Location ?? "Amman, Jordan",
                        CustomerId = customer.CustomerId,
                        TotalPrice = 0m
                    };

                    decimal total = 0m;
                    var orderItems = new List<OrderItem>();
                    foreach (var product in pickedProducts)
                    {
                        var qty = rng.Next(1, 3);
                        var itemTotal = product.ProductPrice * qty;
                        total += itemTotal;

                        orderItems.Add(new OrderItem
                        {
                            OrderItemId = Guid.NewGuid(),
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            ItemQuantity = qty,
                            ItemTotalPrice = itemTotal
                        });
                    }

                    order.TotalPrice = total;

                    await context.Orders.AddAsync(order);
                    await context.OrderItems.AddRangeAsync(orderItems);
                }
            }

            // ---------- Testimonials ----------
            var testimonialTexts = new[]
            {
                "The Rolex I bought exceeded my expectations, fast shipping and great packaging!",
                "Excellent customer service and a beautiful selection of watches.",
                "My Apple Watch arrived quickly and works perfectly. Highly recommend this store.",
                "Great prices on Tissot watches, will definitely shop here again.",
                "The whole ordering process was smooth from browsing to checkout."
            };

            for (int i = 0; i < testimonialTexts.Length && i < customers.Count; i++)
            {
                await context.Testimonials.AddAsync(new Testimonial
                {
                    TestimonialId = Guid.NewGuid(),
                    CustomerTestimonial = testimonialTexts[i],
                    IsApproved = i % 2 == 0,
                    CustomerId = customers[i].CustomerId
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
