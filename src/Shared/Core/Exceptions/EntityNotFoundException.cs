using System.Collections;

namespace Shared.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityName, object? entityId)
        : base($"Entity '{entityName}' with identifier '{FormatIdentifier(entityId)}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    private static string FormatIdentifier(object? entityId)
    {
        if (entityId == null) return "null";

        if (entityId is object[] array)
        {
            return $"[{string.Join(", ", array)}]";
        }

        if (entityId is IEnumerable enumerable && !(entityId is string))
        {
            var items = new List<string>();
            foreach (var item in enumerable)
            {
                items.Add(item?.ToString() ?? "null");
            }
            return $"[{string.Join(", ", items)}]";
        }

        return entityId.ToString() ?? "null";
    }

    public EntityNotFoundException(string message)
        : base(message)
    {
        EntityName = string.Empty;
        EntityId = null;
    }
}
