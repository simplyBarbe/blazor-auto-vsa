using System.Reflection;
using System.Runtime.CompilerServices;

namespace Shared.Core.CRUD;

/// <summary>Extracts key values from commands/queries; supports IEntityKeyProvider, simple types, tuples, and records.</summary>
public class KeyExtractor
{
    private static KeyExtractor? _default;

    public static KeyExtractor Default => _default ??= new KeyExtractor();

    public virtual object[] GetKeyValues(object commandOrQuery)
    {
        if (commandOrQuery == null)
        {
            throw new ArgumentNullException(nameof(commandOrQuery));
        }

        if (commandOrQuery is IEntityKeyProvider keyProvider)
        {
            return keyProvider.GetKeys();
        }

        return GetKeyValuesFromObject(commandOrQuery);
    }

    protected virtual object[] GetKeyValuesFromObject(object key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (IsSimpleType(key))
        {
            return new object[] { key };
        }

        if (key is ITuple tuple)
        {
            return ExtractFromTuple(tuple);
        }

        return ExtractFromObject(key);
    }

    protected virtual bool IsSimpleType(object key)
    {
        return key is int || key is Guid || key is string || key is long || 
               key is short || key is byte || key is uint || key is ushort || 
               key is ulong || key is sbyte || key is decimal || key is double || 
               key is float || key is bool || key is char;
    }

    protected virtual object[] ExtractFromTuple(ITuple tuple)
    {
        var result = new object[tuple.Length];
        for (int i = 0; i < tuple.Length; i++)
        {
            var value = tuple[i];
            if (value == null)
            {
                throw new ArgumentException($"Tuple element at index {i} is null.", nameof(tuple));
            }
            result[i] = value;
        }
        return result;
    }

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
