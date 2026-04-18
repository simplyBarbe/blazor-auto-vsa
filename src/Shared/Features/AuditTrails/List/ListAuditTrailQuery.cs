using Shared.Core;
using Shared.Core.CRUD;
using Shared.Domain.Enums;
using Shared.Features.AuditTrails.Responses;

namespace Shared.Features.AuditTrails.List;

public class ListAuditTrailQuery : IRequest<PagedResult<AuditTrailResponse>>, IPageableQuery
{
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public AuditType? AuditType { get; set; }

    /// <summary>Sort field: TableName, AuditType, or DateTime.</summary>
    public string? SortBy { get; set; }

    public bool? SortAscending { get; set; }
}
