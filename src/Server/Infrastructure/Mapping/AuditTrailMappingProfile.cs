using AutoMapper;
using Server.Domain;
using Shared.Features.AuditTrails.Responses;

namespace Server.Infrastructure.Mapping;

public class AuditTrailMappingProfile : Profile
{
    public AuditTrailMappingProfile()
    {
        CreateMap<AuditTrail, AuditTrailResponse>();
    }
}
