using Client.Dispatching;
using Shared.Core.Auth;

namespace Client.Features.Auth;

/// <summary>
/// Route definitions for authentication endpoints.
/// </summary>
public class AuthRoutes : IRouteDefinition
{
    public void Define(IRouteMap map)
    {
        map.Map<LoginRequest>("/api/auth/login", HttpMethod.Post);
        map.Map<LogoutCommand>("/api/auth/logout", HttpMethod.Post);
    }
}
