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
        Console.WriteLine($"AddFluentValidation for EditContext (Model: {editContext.Model.GetType().Name})");
        var messages = editContext.GetMessageStore();

        editContext.OnValidationRequested += (sender, eventArgs) =>
        {
            Console.WriteLine("OnValidationRequested triggered");
            ValidateModel((EditContext)sender!, messages, serviceProvider);
        };

        editContext.OnFieldChanged += (sender, eventArgs) =>
        {
            Console.WriteLine($"OnFieldChanged triggered for {eventArgs.FieldIdentifier.FieldName}");
            var fieldIdentifier = eventArgs.FieldIdentifier;
            ValidateField((EditContext)sender!, messages, fieldIdentifier, serviceProvider);
        };
    }

    private static void ValidateModel(EditContext editContext, ValidationMessageStore messages, IServiceProvider serviceProvider)
    {
        Console.WriteLine("ValidateModel called");
        var validator = GetValidatorForModel(editContext.Model, serviceProvider);
        if (validator == null)
        {
            Console.WriteLine("No validator found for model");
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
            Console.WriteLine($"Synchronous validation result: {validationResult.IsValid}");
        }
        catch (AsyncValidatorInvokedSynchronouslyException ex)
        {
            Console.WriteLine($"AsyncValidatorInvokedSynchronouslyException caught: {ex.Message}");
            // If validator has async rules, skip client-side validation
            // Server-side validation will catch these errors
            messages.Clear();
            editContext.NotifyValidationStateChanged();
            return;
        }

        messages.Clear(); // Clear messages BEFORE adding new ones from validationResult

        foreach (var error in validationResult.Errors)
        {
            Console.WriteLine($"Validation error: {error.PropertyName} - {error.ErrorMessage}");
            var fieldIdentifier = new FieldIdentifier(editContext.Model, error.PropertyName);
            messages.Add(fieldIdentifier, error.ErrorMessage);
        }

        editContext.NotifyValidationStateChanged();
    }

    private static void ValidateField(EditContext editContext, ValidationMessageStore messages, FieldIdentifier fieldIdentifier, IServiceProvider serviceProvider)
    {
        Console.WriteLine($"ValidateField called for {fieldIdentifier.FieldName}");
        var validator = GetValidatorForModel(editContext.Model, serviceProvider);
        if (validator == null)
        {
            Console.WriteLine($"No validator found for field {fieldIdentifier.FieldName}");
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
            Console.WriteLine($"Field validation result for {fieldIdentifier.FieldName}: {validationResult.IsValid}");
        }
        catch (AsyncValidatorInvokedSynchronouslyException ex)
        {
            Console.WriteLine($"AsyncValidatorInvokedSynchronouslyException in ValidateField: {ex.Message}");
            // If validator has async rules, skip client-side validation for this field
            // Server-side validation will catch these errors
            messages.Clear(fieldIdentifier);
            editContext.NotifyValidationStateChanged();
            return;
        }

        messages.Clear(fieldIdentifier);

        var fieldErrors = validationResult.Errors
            .Where(e => e.PropertyName == fieldIdentifier.FieldName).ToList();

        Console.WriteLine($"Found {fieldErrors.Count} errors for field {fieldIdentifier.FieldName}");
        foreach (var error in fieldErrors)
        {
            Console.WriteLine($"Field error: {error.ErrorMessage}");
            messages.Add(fieldIdentifier, error.ErrorMessage);
        }

        editContext.NotifyValidationStateChanged();
    }

    private static IValidator? GetValidatorForModel(object model, IServiceProvider serviceProvider)
    {
        var modelType = model.GetType();
        Console.WriteLine($"Getting validator for model type: {modelType.Name}");
        var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
        var validator = serviceProvider.GetService(validatorType) as IValidator;
        Console.WriteLine($"Validator found: {validator != null}");
        return validator;
    }
}
