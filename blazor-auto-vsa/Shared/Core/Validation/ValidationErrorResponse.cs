namespace Shared.Core.Validation;

/// <summary>
/// Response model for validation errors returned from API endpoints.
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// Collection of validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();
}
