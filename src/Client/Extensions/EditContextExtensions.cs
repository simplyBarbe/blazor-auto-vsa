using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;

namespace Client.Extensions;

public static class EditContextExtensions
{
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
            messages.Clear();
            editContext.NotifyValidationStateChanged();
            return;
        }

        var context = new ValidationContext<object>(editContext.Model);

        FluentValidation.Results.ValidationResult validationResult;

        try
        {
            validationResult = validator.Validate(context);
        }
        catch (AsyncValidatorInvokedSynchronouslyException)
        {
            messages.Clear();
            editContext.NotifyValidationStateChanged();
            return;
        }

        messages.Clear();

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
            messages.Clear(fieldIdentifier);
            editContext.NotifyValidationStateChanged();
            return;
        }

        var context = new ValidationContext<object>(editContext.Model);

        FluentValidation.Results.ValidationResult validationResult;

        try
        {
            validationResult = validator.Validate(context);
        }
        catch (AsyncValidatorInvokedSynchronouslyException)
        {
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
