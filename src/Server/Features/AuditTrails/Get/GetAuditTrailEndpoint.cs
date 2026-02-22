using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.AuditTrails.Get;
using Shared.Features.AuditTrails.Responses;

namespace Server.Features.AuditTrails.Get;

public class GetAuditTrailEndpoint : GetEntityEndpointBase<long, GetAuditTrailQuery, AuditTrailResponse>
{
    protected override string GetRoute() => "/api/audit-trails/{key:long}";

    protected override GetAuditTrailQuery CreateQuery(long key)
    {
        return new GetAuditTrailQuery(key);
    }
}
