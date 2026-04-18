using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Core;
using Shared.Features.AuditTrails.Responses;
using Shared.Features.AuditTrails.List;
using Client.Components.Base;
using Shared.Domain.Enums;

namespace Client.Features.AuditTrails.Components;

public partial class AuditTrails : BaseComponent
{
    private const int ItemsPerPage = 10;

    protected ListAuditTrailQuery Query { get; } = new();
    protected PagedGridController<AuditTrailResponse> GridController { get; private set; } = default!;
    protected PagedDataGrid<AuditTrailResponse>? Grid { get; set; }

    private IEnumerable<string> _auditTypeOptions = Enum.GetNames<AuditType>().Prepend("All");

    [PersistentState]
    public List<AuditTrailResponse>? RestoredItems { get; set; }

    [PersistentState]
    public int RestoredTotalCount { get; set; }

    private string _selectedAuditTypeString = "All";
    private string SelectedAuditTypeString
    {
        get => _selectedAuditTypeString;
        set
        {
            if (_selectedAuditTypeString != value)
            {
                _selectedAuditTypeString = value;
                Query.AuditType = Enum.TryParse<AuditType>(value, out var result) ? result : null;
            }
        }
    }

    protected override void OnInitialized()
    {
        GridController = new PagedGridController<AuditTrailResponse>(
            FetchAuditTrailsAsync,
            ItemsPerPage,
            RestoredItems,
            RestoredTotalCount,
            (items, totalCount) =>
            {
                RestoredItems = items.ToList();
                RestoredTotalCount = totalCount;
            });

        Track(GridController.State);
    }

    private Task<PagedResult<AuditTrailResponse>?> FetchAuditTrailsAsync(int pageNumber, int pageSize)
    {
        Query.PageNumber = pageNumber;
        Query.PageSize = pageSize;
        return SendAsync(Query);
    }

    private void OnSearchTermChanged(ChangeEventArgs e)
    {
        Query.SearchTerm = e.Value?.ToString();
    }

    private async Task OnApplyFilterAsync()
    {
        if (Grid != null) await Grid.RefreshAsync(resetToFirstPage: true);
    }

    private async Task OnViewDetailsAsync(AuditTrailResponse auditTrail)
    {
        await DialogService.ShowDialogAsync<AuditTrailDialog>(auditTrail, new DialogParameters
        {
            Title = $"Audit Details - {auditTrail.TableName} ({auditTrail.AuditType})",
            Width = "800px",
            TrapFocus = true,
            Modal = true,
            PreventScroll = true
        });
    }
}
