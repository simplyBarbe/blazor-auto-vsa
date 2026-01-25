using System.Reflection;

namespace Shared.Core.CRUD;

/// <summary>
/// Helper class for extracting keys from entity key providers.
/// </summary>
public static class EntityKeyProviderHelper
{
    /// <summary>
    /// Gets key values by looking for an "Id" property in the object.
    /// </summary>
    /// <param name="keyProvider">The object that may have an "Id" property.</param>
    /// <returns>An array of key values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no "Id" property is found.</exception>
    public static object[] GetKeysFromIdProperty(object keyProvider)
    {
        var type = keyProvider.GetType();
        var idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        
        if (idProperty != null && idProperty.CanRead)
        {
            var value = idProperty.GetValue(keyProvider);
            if (value != null)
            {
                return new object[] { value };
            }
        }
        
        throw new InvalidOperationException($"Command/Query must have an 'Id' property or override GetKeys() method. Type: {type.Name}");
    }
}
