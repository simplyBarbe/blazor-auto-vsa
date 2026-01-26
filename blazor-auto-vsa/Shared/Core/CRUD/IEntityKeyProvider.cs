namespace Shared.Core.CRUD;

/// <summary>
/// Interface for commands and queries that provide entity key values.
/// </summary>
public interface IEntityKeyProvider
{
    /// <summary>
    /// Gets the key values for this command or query.
    /// </summary>
    /// <returns>An array of key values.</returns>
    object[] GetKeys();
}
