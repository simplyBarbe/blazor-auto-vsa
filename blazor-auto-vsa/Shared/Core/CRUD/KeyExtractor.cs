using System.Reflection;
using System.Runtime.CompilerServices;

namespace Shared.Core.CRUD;

/// <summary>
/// Utility class for extracting key values from various key types, including composite keys.
/// Can be extended by creating a derived class and overriding extraction methods.
/// </summary>
public class KeyExtractor
{
    private static KeyExtractor? _default;

    /// <summary>
    /// Gets the default instance of KeyExtractor.
    /// </summary>
    public static KeyExtractor Default => _default ??= new KeyExtractor();

    /// <summary>
    /// Extracts key values from a command or query object.
    /// First checks if the object implements IEntityKeyProvider and calls GetKeys().
    /// Otherwise, checks for an "Id" property, then falls back to treating the object as a key.
    /// </summary>
    /// <param name="commandOrQuery">The command or query object to extract keys from.</param>
    /// <returns>An array of key values.</returns>
    public virtual object[] GetKeyValues(object commandOrQuery)
    {
        if (commandOrQuery == null)
        {
            throw new ArgumentNullException(nameof(commandOrQuery));
        }

        // First, check if it implements IEntityKeyProvider (most common case)
        if (commandOrQuery is IEntityKeyProvider keyProvider)
        {
            return keyProvider.GetKeys();
        }

        var type = commandOrQuery.GetType();

        // Fallback: check for "Id" property (exact name match)
        var idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null && idProperty.CanRead)
        {
            var idValue = idProperty.GetValue(commandOrQuery);
            if (idValue != null)
            {
                return new object[] { idValue };
            }
        }

        // Final fallback: treat the object itself as a key (for simple types or tuples)
        return GetKeyValuesFromObject(commandOrQuery);
    }

    /// <summary>
    /// Converts a key object to an object array for repository operations.
    /// Supports simple types (int, Guid, string) and composite types (tuples, records).
    /// </summary>
    /// <param name="key">The key to extract values from.</param>
    /// <returns>An array of key values.</returns>
    protected virtual object[] GetKeyValuesFromObject(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        // Handle simple types
        if (IsSimpleType(key))
        {
            return new object[] { key };
        }

        // Handle tuples
        if (key is ITuple tuple)
        {
            return ExtractFromTuple(tuple);
        }

        // Handle records and classes with properties
        return ExtractFromObject(key);
    }

    /// <summary>
    /// Determines if the key is a simple type. Can be overridden to add custom simple types.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the key is a simple type.</returns>
    protected virtual bool IsSimpleType(object key)
    {
        return key is int || key is Guid || key is string || key is long || 
               key is short || key is byte || key is uint || key is ushort || 
               key is ulong || key is sbyte || key is decimal || key is double || 
               key is float || key is bool || key is char;
    }

    /// <summary>
    /// Extracts values from a tuple. Can be overridden for custom tuple handling.
    /// </summary>
    /// <param name="tuple">The tuple to extract from.</param>
    /// <returns>An array of key values.</returns>
    protected virtual object[] ExtractFromTuple(ITuple tuple)
    {
        var values = new List<object>();
        var length = tuple.Length;
        for (int i = 0; i < length; i++)
        {
            var value = tuple[i];
            if (value == null)
            {
                throw new ArgumentException($"Tuple element at index {i} is null.", nameof(tuple));
            }
            values.Add(value);
        }
        return values.ToArray();
    }

    /// <summary>
    /// Extracts values from an object using reflection. Can be overridden for custom object handling.
    /// </summary>
    /// <param name="key">The key object to extract from.</param>
    /// <returns>An array of key values.</returns>
    protected virtual object[] ExtractFromObject(object key)
    {
        var type = key.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .OrderBy(p => p.Name)
            .ToArray();

        if (properties.Length == 0)
        {
            throw new ArgumentException($"Key type {type.Name} does not have any readable properties.", nameof(key));
        }

        var keyValues = new List<object>();
        foreach (var property in properties)
        {
            var value = property.GetValue(key);
            if (value == null)
            {
                throw new ArgumentException($"Property {property.Name} of key type {type.Name} is null.", nameof(key));
            }
            keyValues.Add(value);
        }

        return keyValues.ToArray();
    }
}
