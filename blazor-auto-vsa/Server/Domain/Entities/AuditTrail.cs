using Server.Domain.Common;
using Server.Domain.Enums;

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

    // New: Transient properties for handling temporary values (not persisted)
    public List<TemporaryProperty> TemporaryProperties { get; } = new();

    public IAuditableEntity? AuditedEntity { get; set; } // Transient reference to the entity being audited

    public string? ErrorMessage { get; set; }
}

// Simple domain class (or use a record: public record TemporaryProperty(string Name, bool IsPrimaryKey);)
public class TemporaryProperty
{
    public string Name { get; set; } = null!;
    public bool IsPrimaryKey { get; set; }
}