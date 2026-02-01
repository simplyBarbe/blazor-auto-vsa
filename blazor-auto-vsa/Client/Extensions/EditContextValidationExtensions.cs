using Microsoft.AspNetCore.Components.Forms;
using Shared.Core.Validation;

namespace Client.Extensions;

/// <summary>
/// Helper extension methods for adding validation messages to EditContext.
/// </summary>
public static class EditContextValidationExtensions
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<EditContext, ValidationMessageStore> _messageStores = new();

    /// <summary>
    /// Gets or creates a ValidationMessageStore for the given EditContext.
    /// </summary>
    internal static ValidationMessageStore GetMessageStore(this EditContext editContext)
    {
        lock (_messageStores)
        {
            if (!_messageStores.TryGetValue(editContext, out var store))
            {
                store = new ValidationMessageStore(editContext);
                _messageStores.Add(editContext, store);
            }

            return store;
        }
    }

    /// <summary>
    /// Adds multiple validation errors to the EditContext.
    /// </summary>
    public static void AddValidationErrors(this EditContext editContext, List<ValidationError> errors)
    {
        var messages = GetMessageStore(editContext);
        
        foreach (var error in errors)
        {
            if (!string.IsNullOrEmpty(error.PropertyName))
            {
                var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
                messages.Add(fieldIdentifier, error.ErrorMessage);
            }
            else
            {
                // General error - add to the whole model (empty property name)
                messages.Add(new FieldIdentifier(editContext.Model, string.Empty), error.ErrorMessage);
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Clears all validation messages from the EditContext.
    /// </summary>
    public static void ClearValidationMessages(this EditContext editContext)
    {
        var messages = GetMessageStore(editContext);
        messages.Clear();
        editContext.NotifyValidationStateChanged();
    }
}
