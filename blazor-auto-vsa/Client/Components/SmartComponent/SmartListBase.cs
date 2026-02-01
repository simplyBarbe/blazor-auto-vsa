using Shared.Core;
using System.Linq;

namespace Client.Components.SmartComponent;

/// <summary>
/// Base class for list components that handle pagination and data loading.
/// </summary>
/// <typeparam name="TResponse">The type of items in the list.</typeparam>
/// <typeparam name="TQuery">The type of the query used to fetch data.</typeparam>
public abstract class SmartListBase<TResponse, TQuery> : SmartComponentBase
    where TQuery : IRequest<PagedResult<TResponse>>, new()
{
    /// <summary>
    /// The current set of items to display.
    /// </summary>
    protected IQueryable<TResponse>? Items { get; private set; }

    /// <summary>
    /// The query used for data loading, including pagination and filtering parameters.
    /// </summary>
    protected TQuery Query { get; set; } = new();

    /// <summary>
    /// The total count of items across all pages.
    /// </summary>
    protected int TotalCount { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Loads data using the current Query parameters.
    /// </summary>
    protected virtual async Task LoadDataAsync()
    {
        var result = await SendAsync(Query);
        if (result != null)
        {
            Items = result.Items.AsQueryable();
            TotalCount = result.TotalCount;
        }
    }

    /// <summary>
    /// Helper to determine if we are running in the browser (WebAssembly).
    /// </summary>
    protected bool IsBrowser => OperatingSystem.IsBrowser();
}
