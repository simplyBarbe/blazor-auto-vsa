using Shared.Core;
using Shared.Core.Pipeline;
using Shared.Core.Validation;

namespace Server.Infrastructure.Pipeline;

/// <summary>
/// Pipeline behavior that validates requests before handler execution.
/// If no validator is registered for the request type, validation is skipped.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAsyncRequestValidator _validator;

    public ValidationBehavior(IAsyncRequestValidator validator)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await next();
    }
}
