namespace Shared.Core.CRUD;

/// <summary>
/// Base query for listing entities with pagination and filtering.
/// </summary>
/// <typeparam name="TResponse">The type of response items.</typeparam>
public abstract class ListEntityQuery<TResponse> : IRequest<PagedResult<TResponse>>
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
