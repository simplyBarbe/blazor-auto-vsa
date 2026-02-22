using FluentValidation;

namespace Shared.Core.Validation;

/// <summary>
/// Implementation of IAsyncRequestValidator that uses FluentValidation validators.
/// </summary>
public class FluentValidationRequestValidator : IAsyncRequestValidator
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationRequestValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ValidationResult> ValidateAsync(object request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);

        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator == null)
        {
            // No validator registered for this type - consider it valid
            return ValidationResult.Success();
        }

        var context = new ValidationContext<object>(request);
        var fluentResult = await validator.ValidateAsync(context, cancellationToken);

        if (fluentResult.IsValid)
        {
            return ValidationResult.Success();
        }

        var errors = fluentResult.Errors
            .Select(e => new ValidationError
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage
            })
            .ToList();

        return ValidationResult.Failure(errors);
    }
}
