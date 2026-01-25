namespace Shared.Core.Validation;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// The validation errors that caused this exception.
    /// </summary>
    public List<ValidationError> Errors { get; }

    /// <summary>
    /// Creates a new ValidationException with the specified errors.
    /// </summary>
    public ValidationException(List<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a new ValidationException with a single error.
    /// </summary>
    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new List<ValidationError>
        {
            new() { PropertyName = propertyName, ErrorMessage = errorMessage }
        };
    }
}
