namespace Shared.Core.Auth;

/// <summary>
/// Represents the authenticated user's information.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// The user's unique identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Additional user claims as key-value pairs.
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();
}
