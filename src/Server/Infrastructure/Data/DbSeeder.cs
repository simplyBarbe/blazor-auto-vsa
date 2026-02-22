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

        context.Products.AddRange(
            new Product { Name = "Seed Product Alpha", Price = 19.99m },
            new Product { Name = "Seed Product Beta", Price = 29.99m }
        );
        await context.SaveChangesAsync();
    }

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
