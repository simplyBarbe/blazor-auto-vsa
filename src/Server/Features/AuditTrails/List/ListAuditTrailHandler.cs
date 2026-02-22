using AutoMapper;
using Server.Domain;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Features.AuditTrails.List;
using Shared.Features.AuditTrails.Responses;
using System.Linq.Expressions;

namespace Server.Features.AuditTrails.List;

public class ListAuditTrailHandler : ListEntityHandlerBase<AuditTrail, ListAuditTrailQuery, AuditTrailResponse>
{
    public ListAuditTrailHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
    {
    }

    protected override QueryFilter<AuditTrail> BuildQueryFilter(ListAuditTrailQuery query)
    {
        var pageNumber = query.PageNumber ?? 1;
        var pageSize = query.PageSize ?? 10;

        var filter = new QueryFilter<AuditTrail>
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize,
            OrderBy = new List<SortExpression<AuditTrail>>
            {
                new(p => p.DateTime, SortDirection.Descending)
            }
        };

        var predicates = new List<Expression<Func<AuditTrail, bool>>>();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            predicates.Add(p => p.TableName != null && p.TableName.Contains(query.SearchTerm));
        }

        if (query.AuditType.HasValue)
        {
            predicates.Add(p => p.AuditType == query.AuditType.Value);
        }

        if (predicates.Any())
        {
            filter.Filters = predicates;
        }

        return filter;
    }
}
