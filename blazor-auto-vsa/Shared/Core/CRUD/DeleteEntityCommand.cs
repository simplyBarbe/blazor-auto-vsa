namespace Shared.Core.CRUD;

/// <summary>
/// Base command for deleting an entity.
/// Supports simple types (int, Guid, string) and composite keys (tuples, records).
/// </summary>
public abstract class DeleteEntityCommand : IRequest<object?>, IEntityKeyProvider
{
    /// <summary>
    /// Gets the key values for this command.
    /// Default implementation looks for an "Id" property, or can be overridden for custom key extraction.
    /// </summary>
    /// <returns>An array of key values.</returns>
    public virtual object[] GetKeys()
    {
        return EntityKeyProviderHelper.GetKeysFromIdProperty(this);
    }
}
