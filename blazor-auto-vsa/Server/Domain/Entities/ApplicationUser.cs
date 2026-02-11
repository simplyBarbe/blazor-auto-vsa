using Microsoft.AspNetCore.Identity;

namespace Server.Domain.Entities;

/// <summary>
/// Represents an application user with authentication and identity information.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Add custom properties here if needed in the future
    // For example: public string FirstName { get; set; } = string.Empty;
}
