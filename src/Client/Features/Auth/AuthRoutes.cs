using Client.Dispatching;
using Shared.Core.Auth;

namespace Client.Features.Auth;

public sealed class AuthRoutes : IRouteDefinition
{
    public void Define(RequestEndpointMapper routes)
    {
        routes.Map<LoginRequest>("/api/auth/login", HttpMethod.Post);
        routes.Map<LogoutCommand>("/api/auth/logout", HttpMethod.Post);
    }
}
