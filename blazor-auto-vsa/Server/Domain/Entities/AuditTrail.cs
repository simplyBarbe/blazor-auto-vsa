using Server.Domain.Common;
using Shared.Domain.Enums;

namespace Server.Domain;

public class AuditTrail
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

    public List<TemporaryProperty> TemporaryProperties { get; } = new();
    public IAuditableEntity? AuditedEntity { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TemporaryProperty
{
    public string Name { get; set; } = null!;
    public bool IsPrimaryKey { get; set; }
}