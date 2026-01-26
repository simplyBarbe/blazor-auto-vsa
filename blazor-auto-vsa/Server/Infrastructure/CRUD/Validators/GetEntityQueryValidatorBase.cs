using FluentValidation;
using Shared.Core.CRUD;
using System.Reflection;

namespace Server.Infrastructure.CRUD.Validators;

/// <summary>
/// Base validator for get entity queries.
/// Validates that either an "Id" property exists and is set, or GetKeys() returns valid keys.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
public abstract class GetEntityQueryValidatorBase<TQuery> : AbstractValidator<TQuery>
    where TQuery : IEntityKeyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityQueryValidatorBase{TQuery}"/> class.
    /// </summary>
    protected GetEntityQueryValidatorBase()
    {
        var type = typeof(TQuery);
        var idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (idProperty != null && idProperty.CanRead)
        {
            // Validate "Id" property if it exists
            RuleFor(x => idProperty.GetValue(x))
                .NotNull()
                .WithMessage("The Id is required.");
        }
        else
        {
            // If no "Id" property, validate GetKeys() returns valid keys
            RuleFor(x => x.GetKeys())
                .NotEmpty()
                .WithMessage("The key is required.");
        }
    }
}
