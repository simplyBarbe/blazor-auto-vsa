using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Domain.Entities;

namespace Server.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        var misc = await EnsureGroupAsync(context, "General", "Misc");
        var tv = await EnsureGroupAsync(context, "Electronics", "TV");
        var laptops = await EnsureGroupAsync(context, "Electronics", "Laptops");
        var audio = await EnsureGroupAsync(context, "Electronics", "Audio");
        var officeSupplies = await EnsureGroupAsync(context, "Office", "Supplies");
        var kitchen = await EnsureGroupAsync(context, "Home", "Kitchen");

        var seedProducts = GetSeedProducts(
            miscGroupId: misc.Id,
            tvGroupId: tv.Id,
            laptopGroupId: laptops.Id,
            audioGroupId: audio.Id,
            officeSuppliesGroupId: officeSupplies.Id,
            kitchenGroupId: kitchen.Id
        ).ToList();

        if (!await context.Products.AnyAsync())
        {
            context.Products.AddRange(seedProducts);
            await context.SaveChangesAsync();
            return;
        }

        var existingNames = await context.Products
            .Select(p => p.Name)
            .ToListAsync();

        var missingProducts = seedProducts
            .Where(p => !existingNames.Contains(p.Name))
            .ToList();

        if (missingProducts.Count > 0)
        {
            context.Products.AddRange(missingProducts);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ProductGroup> EnsureGroupAsync(
        ApplicationDbContext context,
        string categoryName,
        string groupName)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
        if (category is null)
        {
            category = new Category { Name = categoryName };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }

        var group = await context.ProductGroups.FirstOrDefaultAsync(g =>
            g.CategoryId == category.Id && g.Name == groupName);
        if (group is null)
        {
            group = new ProductGroup { CategoryId = category.Id, Name = groupName };
            context.ProductGroups.Add(group);
            await context.SaveChangesAsync();
        }

        return group;
    }

    /// <summary>
    /// Small deterministic sample catalog for local demos (lists, paging).
    /// </summary>
    private static IEnumerable<Product> GetSeedProducts(
        int miscGroupId,
        int tvGroupId,
        int laptopGroupId,
        int audioGroupId,
        int officeSuppliesGroupId,
        int kitchenGroupId) =>
    [
        new Product { GroupId = miscGroupId, Name = "Bluetooth noise-cancelling headphones", Price = 89.90m },
        new Product { GroupId = miscGroupId, Name = "Ergonomic wireless mouse", Price = 34.50m },
        new Product { GroupId = miscGroupId, Name = "USB-C 7-in-1 hub", Price = 45.99m },
        new Product { GroupId = miscGroupId, Name = "LED desk lamp", Price = 42.00m },
        new Product { GroupId = miscGroupId, Name = "Stainless steel water bottle 750ml", Price = 24.90m },
        new Product { GroupId = miscGroupId, Name = "Yoga mat 6mm", Price = 27.90m },
        new Product { GroupId = miscGroupId, Name = "Eraser", Price = 0.99m },
        new Product { GroupId = tvGroupId, Name = "27-inch QHD IPS 165Hz monitor", Price = 329.00m },
        new Product { GroupId = tvGroupId, Name = "Hisense 27''", Price = 499.99m },
        new Product { GroupId = tvGroupId, Name = "Samsung 55-inch 4K Smart TV", Price = 799.00m },
        new Product { GroupId = tvGroupId, Name = "LG 65-inch OLED Smart TV", Price = 1399.00m },
        new Product { GroupId = laptopGroupId, Name = "Ultrabook 14-inch i7 16GB RAM", Price = 1199.00m },
        new Product { GroupId = laptopGroupId, Name = "Gaming laptop 15-inch RTX 4060", Price = 1499.00m },
        new Product { GroupId = laptopGroupId, Name = "Budget laptop 15-inch i5 8GB RAM", Price = 649.00m },
        new Product { GroupId = audioGroupId, Name = "Portable Bluetooth speaker", Price = 59.90m },
        new Product { GroupId = audioGroupId, Name = "Over-ear studio headphones", Price = 129.00m },
        new Product { GroupId = audioGroupId, Name = "Soundbar with wireless subwoofer", Price = 249.00m },
        new Product { GroupId = officeSuppliesGroupId, Name = "A4 copy paper 500 sheets", Price = 6.90m },
        new Product { GroupId = officeSuppliesGroupId, Name = "Fine-tip black pens (pack of 10)", Price = 4.50m },
        new Product { GroupId = officeSuppliesGroupId, Name = "Hardcover notebook A5", Price = 8.20m },
        new Product { GroupId = kitchenGroupId, Name = "Non-stick frying pan 28cm", Price = 29.99m },
        new Product { GroupId = kitchenGroupId, Name = "Electric kettle 1.7L", Price = 39.90m },
        new Product { GroupId = kitchenGroupId, Name = "Chef knife 20cm", Price = 34.90m }
    ];

    public static async Task SeedIdentityAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
