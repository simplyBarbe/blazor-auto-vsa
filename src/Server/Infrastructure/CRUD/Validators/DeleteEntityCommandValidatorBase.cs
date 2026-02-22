using FluentValidation;
using Shared.Core.CRUD;
using System.Reflection;

namespace Server.Infrastructure.CRUD.Validators;

/// <summary>Validates delete: either "Id" is set or GetKeys() returns valid keys.</summary>
public abstract class DeleteEntityCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IEntityKeyProvider
{
    protected DeleteEntityCommandValidatorBase()
    {
        var type = typeof(TCommand);
        var idProperty = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        if (idProperty != null && idProperty.CanRead)
        {
            RuleFor(x => idProperty.GetValue(x))
                .NotNull()
                .WithMessage("The Id is required.");
        }
        else
        {
            RuleFor(x => x.GetKeys())
                .NotEmpty()
                .WithMessage("The key is required.");
        }
    }
}
