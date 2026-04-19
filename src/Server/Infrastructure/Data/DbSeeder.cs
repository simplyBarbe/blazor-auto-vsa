using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Domain.Entities;

namespace Server.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        await EnsureGeneralMiscTaxonomyAsync(context);
        var misc = await EnsureMiscGroupAsync(context);
        var tv = await EnsureTvGroupAsync(context);

        if (!await context.Products.AnyAsync())
        {
            context.Products.AddRange(GetSeedProducts(misc.Id, tv.Id));
            await context.SaveChangesAsync();
            return;
        }

        if (!await context.Products.AnyAsync(p => p.Name == "Hisense 27''"))
        {
            context.Products.Add(new Product
            {
                GroupId = tv.Id,
                Name = "Hisense 27''",
                Price = 499.99m
            });
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Ensures default taxonomy exists (e.g. in-memory / tests where SQL migrations did not insert rows).
    /// </summary>
    private static async Task EnsureGeneralMiscTaxonomyAsync(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync(c => c.Name == "General"))
        {
            context.Categories.Add(new Category { Name = "General" });
            await context.SaveChangesAsync();
        }

        var general = await context.Categories.FirstAsync(c => c.Name == "General");
        if (!await context.ProductGroups.AnyAsync(g => g.CategoryId == general.Id && g.Name == "Misc"))
        {
            context.ProductGroups.Add(new ProductGroup { CategoryId = general.Id, Name = "Misc" });
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ProductGroup> EnsureMiscGroupAsync(ApplicationDbContext context)
    {
        var general = await context.Categories.FirstAsync(c => c.Name == "General");
        return await context.ProductGroups.FirstAsync(g => g.CategoryId == general.Id && g.Name == "Misc");
    }

    private static async Task<ProductGroup> EnsureTvGroupAsync(ApplicationDbContext context)
    {
        var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        if (electronics is null)
        {
            electronics = new Category { Name = "Electronics" };
            context.Categories.Add(electronics);
            await context.SaveChangesAsync();
        }

        var tv = await context.ProductGroups.FirstOrDefaultAsync(g =>
            g.CategoryId == electronics.Id && g.Name == "TV");
        if (tv is null)
        {
            tv = new ProductGroup { CategoryId = electronics.Id, Name = "TV" };
            context.ProductGroups.Add(tv);
            await context.SaveChangesAsync();
        }

        return tv;
    }

    /// <summary>
    /// Small deterministic sample catalog for local demos (lists, paging).
    /// </summary>
    private static IEnumerable<Product> GetSeedProducts(int miscGroupId, int tvGroupId) =>
    [
        new Product { GroupId = miscGroupId, Name = "Bluetooth noise-cancelling headphones", Price = 89.90m },
        new Product { GroupId = miscGroupId, Name = "Ergonomic wireless mouse", Price = 34.50m },
        new Product { GroupId = miscGroupId, Name = "USB-C 7-in-1 hub", Price = 45.99m },
        new Product { GroupId = miscGroupId, Name = "LED desk lamp", Price = 42.00m },
        new Product { GroupId = miscGroupId, Name = "Stainless steel water bottle 750ml", Price = 24.90m },
        new Product { GroupId = miscGroupId, Name = "Yoga mat 6mm", Price = 27.90m },
        new Product { GroupId = miscGroupId, Name = "Eraser", Price = 0.99m },
        new Product { GroupId = tvGroupId, Name = "27-inch QHD IPS 165Hz monitor", Price = 329.00m },
        new Product { GroupId = tvGroupId, Name = "Hisense 27''", Price = 499.99m }
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
