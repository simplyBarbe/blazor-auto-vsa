using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace Client.Extensions;

/// <summary>
/// Extension methods for integrating FluentValidation with Blazor EditContext.
/// Uses Validate() for synchronous validators (client-side) and handles async validators gracefully.
/// </summary>
public static class EditContextExtensions
{
    /// <summary>
    /// Adds FluentValidation support to the EditContext.
    /// </summary>
    public static void AddFluentValidation(this EditContext editContext, IServiceProvider serviceProvider)
    {
        var messages = editContext.GetMessageStore();

        editContext.OnValidationRequested += (sender, eventArgs) =>
        {
            ValidateModel((EditContext)sender!, messages, serviceProvider);
        };

        editContext.OnFieldChanged += (sender, eventArgs) =>
        {
            var fieldIdentifier = eventArgs.FieldIdentifier;
            ValidateField((EditContext)sender!, messages, fieldIdentifier, serviceProvider);
        };
    }

    private static void ValidateModel(EditContext editContext, ValidationMessageStore messages, IServiceProvider serviceProvider)
    {
        var validator = GetValidatorForModel(editContext.Model, serviceProvider);
        if (validator == null)
        {
            // No validator found - clear any existing messages and consider valid
            // Server-side validation will handle validation if needed
            messages.Clear();
            editContext.NotifyValidationStateChanged();
            return;
        }

        var context = new ValidationContext<object>(editContext.Model);

        FluentValidation.Results.ValidationResult validationResult;

        try
        {
            // Try synchronous validation first (for client-side validators)
            validationResult = validator.Validate(context);
        }
        catch (AsyncValidatorInvokedSynchronouslyException)
        {
            // If validator has async rules, skip client-side validation
            // Server-side validation will catch these errors
            messages.Clear();
            editContext.NotifyValidationStateChanged();
            return;
        }

        messages.Clear(); // Clear messages BEFORE adding new ones from validationResult

        foreach (var error in validationResult.Errors)
        {
            var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
            messages.Add(fieldIdentifier, error.ErrorMessage);
        }

        editContext.NotifyValidationStateChanged();
    }

    private static void ValidateField(EditContext editContext, ValidationMessageStore messages, FieldIdentifier fieldIdentifier, IServiceProvider serviceProvider)
    {
        var validator = GetValidatorForModel(editContext.Model, serviceProvider);
        if (validator == null)
        {
            // No validator found - clear field errors and consider valid
            messages.Clear(fieldIdentifier);
            editContext.NotifyValidationStateChanged();
            return;
        }

        var context = new ValidationContext<object>(editContext.Model);

        FluentValidation.Results.ValidationResult validationResult;

        try
        {
            // Try synchronous validation first (for client-side validators)
            validationResult = validator.Validate(context);
        }
        catch (AsyncValidatorInvokedSynchronouslyException)
        {
            // If validator has async rules, skip client-side validation for this field
            // Server-side validation will catch these errors
            messages.Clear(fieldIdentifier);
            editContext.NotifyValidationStateChanged();
            return;
        }

        messages.Clear(fieldIdentifier);

        var fieldErrors = validationResult.Errors
            .Where(e => e.PropertyName == fieldIdentifier.FieldName).ToList();

        foreach (var error in fieldErrors)
        {
            messages.Add(fieldIdentifier, error.ErrorMessage);
        }

        editContext.NotifyValidationStateChanged();
    }

    private static IValidator? GetValidatorForModel(object model, IServiceProvider serviceProvider)
    {
        var modelType = model.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
        var validator = serviceProvider.GetService(validatorType) as IValidator;
        return validator;
    }
}
