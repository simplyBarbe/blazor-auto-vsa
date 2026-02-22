using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Shared.Features.AuditTrails.Responses;
using Shared.Features.AuditTrails.List;
using Client.Components.Base;
using Shared.Domain.Enums;

namespace Client.Features.AuditTrails.Components;

public partial class AuditTrails : PagedListComponent<AuditTrailResponse, ListAuditTrailQuery>
{
    private FluentDataGrid<AuditTrailResponse>? _grid;

    private IEnumerable<string> _auditTypeOptions = Enum.GetNames<AuditType>().Prepend("All");

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

    protected override int ItemsPerPage => 10;

    protected override async Task LoadDataAsync()
    {
        if (_grid != null)
        {
            await _grid.RefreshDataAsync();
        }
        else
        {
            await base.LoadDataAsync();
        }
    }

    private async ValueTask<GridItemsProviderResult<AuditTrailResponse>> AuditTrailProvider(GridItemsProviderRequest<AuditTrailResponse> request)
    {
        return await ProvideItemsAsync(request);
    }

    private void OnSearchTermChanged(ChangeEventArgs e)
    {
        Query.SearchTerm = e.Value?.ToString();
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
