using System.Collections;

namespace Shared.Core.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the entity type.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier of the entity that was not found.
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Creates a new EntityNotFoundException with the specified entity name and identifier.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="entityId">The identifier of the entity that was not found.</param>
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

    /// <summary>
    /// Creates a new EntityNotFoundException with a custom message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public EntityNotFoundException(string message)
        : base(message)
    {
        EntityName = string.Empty;
        EntityId = null;
    }
}
