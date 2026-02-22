using Shared.Domain.Enums;

namespace Shared.Features.AuditTrails.Responses;

public class AuditTrailResponse
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public AuditType AuditType { get; set; }
    public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    public Dictionary<string, object?>? OldValues { get; set; }
    public Dictionary<string, object?>? NewValues { get; set; }
    public List<string>? AffectedColumns { get; set; }
    public Dictionary<string, object> PrimaryKey { get; set; } = new();
}
