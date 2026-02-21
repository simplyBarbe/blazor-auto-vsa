using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.Domain.Entities;
using Shared.Core;
using Shared.Core.Auth;

namespace Server.Infrastructure.Auth;

/// <summary>
/// Authentication API endpoints for login, logout, and user information.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates a user with email and password, returning user information and starting a session");

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout user")
            .WithDescription("Signs out the current authenticated user and ends their session")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user")
            .WithDescription("Retrieves information about the currently authenticated user including roles and claims");
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] IRequestSender sender)
    {
        try
        {
            var userInfo = await sender.Send<UserInfo>(request);
            return Results.Ok(userInfo);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    private static async Task<IResult> Logout(
        [FromServices] IRequestSender sender)
    {
        await sender.Send<object?>(new LogoutCommand());
        return Results.Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        var userInfo = new UserInfo
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            Claims = claims
                .GroupBy(c => c.Type)
                .ToDictionary(group => group.Key, group => group.Last().Value)
        };

        return Results.Ok(userInfo);
    }
}
