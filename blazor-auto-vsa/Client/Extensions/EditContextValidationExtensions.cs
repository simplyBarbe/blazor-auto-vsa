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
        Console.WriteLine($"GetMessageStore for EditContext (Model: {editContext.Model.GetType().Name})");
        lock (_messageStores)
        {
            if (!_messageStores.TryGetValue(editContext, out var store))
            {
                Console.WriteLine("Creating NEW ValidationMessageStore");
                store = new ValidationMessageStore(editContext);
                _messageStores.Add(editContext, store);
            }
            else
            {
                Console.WriteLine("Reusing EXISTING ValidationMessageStore");
            }

            return store;
        }
    }

    /// <summary>
    /// Adds multiple validation errors to the EditContext.
    /// </summary>
    public static void AddValidationErrors(this EditContext editContext, List<ValidationError> errors)
    {
        Console.WriteLine($"AddValidationErrors called with {errors.Count} errors");
        var messages = GetMessageStore(editContext);
        
        foreach (var error in errors)
        {
            Console.WriteLine($"Server error: {error.PropertyName} - {error.ErrorMessage}");
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
        Console.WriteLine("ClearValidationMessages called");
        var messages = GetMessageStore(editContext);
        messages.Clear();
        editContext.NotifyValidationStateChanged();
    }
}
