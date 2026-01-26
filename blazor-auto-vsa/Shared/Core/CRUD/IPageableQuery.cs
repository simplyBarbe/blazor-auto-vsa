namespace Shared.Core.CRUD;

/// <summary>
/// Interface for queries that support pagination.
/// </summary>
public interface IPageableQuery
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    int PageSize { get; set; }
}
