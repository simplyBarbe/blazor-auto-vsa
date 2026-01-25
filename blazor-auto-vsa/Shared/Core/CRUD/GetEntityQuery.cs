namespace Shared.Core.CRUD;

/// <summary>
/// Base query for getting an entity by its key.
/// Supports simple types (int, Guid, string) and composite keys (tuples, records).
/// </summary>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public abstract class GetEntityQuery<TResponse> : IRequest<TResponse>, IEntityKeyProvider
{
    /// <summary>
    /// Gets the key values for this query.
    /// Default implementation looks for an "Id" property, or can be overridden for custom key extraction.
    /// </summary>
    /// <returns>An array of key values.</returns>
    public virtual object[] GetKeys()
    {
        return EntityKeyProviderHelper.GetKeysFromIdProperty(this);
    }
}
