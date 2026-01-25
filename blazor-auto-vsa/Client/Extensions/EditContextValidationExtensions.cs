using Microsoft.AspNetCore.Components.Forms;
using Shared.Core.Validation;

namespace Client.Extensions;

/// <summary>
/// Helper extension methods for adding validation messages to EditContext.
/// </summary>
public static class EditContextValidationExtensions
{
    /// <summary>
    /// Adds multiple validation errors to the EditContext.
    /// </summary>
    public static void AddValidationErrors(this EditContext editContext, List<ValidationError> errors)
    {
        var messages = new ValidationMessageStore(editContext);

        foreach (var error in errors)
        {
            if (!string.IsNullOrEmpty(error.PropertyName))
            {
                var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
                messages.Add(fieldIdentifier, error.ErrorMessage);
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Clears all validation messages from the EditContext.
    /// </summary>
    public static void ClearValidationMessages(this EditContext editContext)
    {
        var messages = new ValidationMessageStore(editContext);
        messages.Clear();
        editContext.NotifyValidationStateChanged();
    }
}
