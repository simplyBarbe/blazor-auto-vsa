using Shared.Core;
using Shared.Core.CRUD;
using Shared.Features.AuditTrails.Responses;

namespace Shared.Features.AuditTrails.Get;

public class GetAuditTrailQuery : IRequest<AuditTrailResponse>, IEntityKeyProvider
{
    public long Id { get; set; }

    public object[] GetKeys() => new object[] { Id };

    public GetAuditTrailQuery(long id)
    {
        Id = id;
    }

    public GetAuditTrailQuery() { }
}
