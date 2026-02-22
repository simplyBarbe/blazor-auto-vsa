using Client.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Client.Components;

public class FluentValidationValidator : ComponentBase
{
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    protected override void OnInitialized()
    {
        if (EditContext == null)
        {
            throw new InvalidOperationException(
                $"{nameof(FluentValidationValidator)} requires a cascading parameter of type {nameof(EditContext)}. " +
                $"For example, you can use {nameof(FluentValidationValidator)} inside an EditForm.");
        }

        EditContext.AddFluentValidation(ServiceProvider);
    }
}
