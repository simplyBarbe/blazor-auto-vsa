using Client.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Client.Components;

/// <summary>
/// Blazor component that integrates FluentValidation with EditForm.
/// Place this component inside an EditForm to enable FluentValidation.
/// </summary>
public class FluentValidationValidator : ComponentBase
{
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    protected override void OnInitialized()
    {
        Console.WriteLine("[FluentValidationValidator] OnInitialized");
        if (EditContext == null)
        {
            Console.WriteLine("[FluentValidationValidator] EditContext is NULL!");
            throw new InvalidOperationException(
                $"{nameof(FluentValidationValidator)} requires a cascading parameter of type {nameof(EditContext)}. " +
                $"For example, you can use {nameof(FluentValidationValidator)} inside an EditForm.");
        }

        Console.WriteLine($"[FluentValidationValidator] Adding FluentValidation to EditContext (Model: {EditContext.Model.GetType().Name})");
        EditContext.AddFluentValidation(ServiceProvider);
    }
}
