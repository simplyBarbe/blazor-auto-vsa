using Shared.Core;
using Shared.Core.Pipeline;
using Shared.Core.Validation;
using System.Linq;

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
        // #region agent log
        try { var requestJson = System.Text.Json.JsonSerializer.Serialize(request); System.IO.File.AppendAllText(@"c:\Users\Andrea\source\repos\blazor-auto-vsa\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "ValidationBehavior.cs:23", message = "Validation starting", data = new { requestType = typeof(TRequest).Name, requestJson }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\Users\Andrea\source\repos\blazor-auto-vsa\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "ValidationBehavior.cs:25", message = "Validation result", data = new { isValid = validationResult.IsValid, errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToArray() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await next();
    }
}
