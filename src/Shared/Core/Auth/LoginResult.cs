namespace Shared.Core.Auth;

public class LoginResult
{
    public bool Succeeded { get; set; }
    public UserInfo? UserInfo { get; set; }
    public string? ErrorMessage { get; set; }
}
