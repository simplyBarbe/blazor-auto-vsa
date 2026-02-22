using Server.Infrastructure.CRUD.Endpoints;
using Shared.Features.AuditTrails.List;
using Shared.Features.AuditTrails.Responses;

namespace Server.Features.AuditTrails.List;

public class ListAuditTrailEndpoint : ListEntityEndpointBase<ListAuditTrailQuery, AuditTrailResponse>
{
    protected override string GetRoute() => "/api/audit-trails";
}
