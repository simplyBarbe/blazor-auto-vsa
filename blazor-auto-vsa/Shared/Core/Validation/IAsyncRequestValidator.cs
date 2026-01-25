namespace Shared.Core.Validation;

/// <summary>
/// Interface for validating requests asynchronously.
/// Supports complex validation scenarios including database lookups.
/// </summary>
public interface IAsyncRequestValidator
{
    /// <summary>
    /// Validates the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ValidationResult indicating success or failure with errors.</returns>
    Task<ValidationResult> ValidateAsync(object request, CancellationToken cancellationToken = default);
}
