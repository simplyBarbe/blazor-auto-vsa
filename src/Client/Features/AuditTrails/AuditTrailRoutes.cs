using Client.Dispatching;
using Shared.Features.AuditTrails.Get;
using Shared.Features.AuditTrails.List;

namespace Client.Features.AuditTrails;

public sealed class AuditTrailRoutes : IRouteDefinition
{
    public void Define(RequestEndpointMapper routes)
    {
        routes.Map<GetAuditTrailQuery>("/api/audit-trails/{Id}", HttpMethod.Get);
        routes.Map<ListAuditTrailQuery>("/api/audit-trails", HttpMethod.Get);
    }
}
