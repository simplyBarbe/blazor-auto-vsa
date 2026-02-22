namespace Shared.Core.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Collection of validation errors, if any.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    public static ValidationResult Failure(List<ValidationError> errors) => new() { IsValid = false, Errors = errors };
}
