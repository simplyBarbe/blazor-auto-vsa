using Client.Dispatching;
using Shared.Features.AuditTrails.Get;
using Shared.Features.AuditTrails.List;

namespace Client.Features.AuditTrails;

public class AuditTrailRoutes : IRouteDefinition
{
    public void Define(IRouteMap map)
    {
        map.Map<GetAuditTrailQuery>("/api/audit-trails/{Id}", HttpMethod.Get);
        map.Map<ListAuditTrailQuery>("/api/audit-trails", HttpMethod.Get);
    }
}
