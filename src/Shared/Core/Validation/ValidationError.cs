namespace Shared.Core.Validation;

/// <summary>
/// Represents a single validation error.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// The name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The error message describing the validation failure.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
