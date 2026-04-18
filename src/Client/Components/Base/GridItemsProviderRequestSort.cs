using Microsoft.FluentUI.AspNetCore.Components;

namespace Client.Components.Base;

/// <summary>
/// Maps FluentDataGrid <see cref="GridItemsProviderRequest{TGridItem}.GetSortByProperties"/> to API query sort fields.
/// </summary>
internal static class GridItemsProviderRequestSort
{
    public static void Apply<TGridItem>(
        GridItemsProviderRequest<TGridItem> request,
        Action<string?, bool?> setSort)
    {
        if (request.SortByColumn is null)
        {
            setSort(null, null);
            return;
        }

        foreach (var pair in request.GetSortByProperties())
        {
            var propertyName = pair.PropertyName;
            var ascending = pair.Direction switch
            {
                SortDirection.Ascending => true,
                SortDirection.Descending => false,
                _ => request.SortByAscending
            };
            setSort(propertyName, ascending);
            return;
        }

        setSort(null, null);
    }
}
