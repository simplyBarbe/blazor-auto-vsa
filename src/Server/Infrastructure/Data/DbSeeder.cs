using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Domain.Entities;

namespace Server.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedProductsAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        context.Products.AddRange(GetSeedProducts());
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Small deterministic sample catalog for local demos (lists, paging).
    /// </summary>
    private static IEnumerable<Product> GetSeedProducts() =>
    [
        new Product { Name = "Bluetooth noise-cancelling headphones", Price = 89.90m },
        new Product { Name = "Ergonomic wireless mouse", Price = 34.50m },
        new Product { Name = "USB-C 7-in-1 hub", Price = 45.99m },
        new Product { Name = "LED desk lamp", Price = 42.00m },
        new Product { Name = "Stainless steel water bottle 750ml", Price = 24.90m },
        new Product { Name = "Yoga mat 6mm", Price = 27.90m },
        new Product { Name = "Eraser", Price = 0.99m },
        new Product { Name = "27-inch QHD IPS 165Hz monitor", Price = 329.00m },
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
