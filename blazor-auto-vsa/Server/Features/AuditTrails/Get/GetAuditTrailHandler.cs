using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.AuditTrails.Get;
using Shared.Features.AuditTrails.Responses;

namespace Server.Features.AuditTrails.Get;

public class GetAuditTrailHandler : GetEntityHandlerBase<AuditTrail, GetAuditTrailQuery, AuditTrailResponse>
{
    public GetAuditTrailHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }
}
