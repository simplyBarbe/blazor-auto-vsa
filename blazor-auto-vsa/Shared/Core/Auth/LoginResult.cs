namespace Shared.Core.Auth;

/// <summary>
/// Result of a login attempt via JS interop.
/// </summary>
public class LoginResult
{
    public bool Succeeded { get; set; }
    public UserInfo? UserInfo { get; set; }
    public string? ErrorMessage { get; set; }
}
