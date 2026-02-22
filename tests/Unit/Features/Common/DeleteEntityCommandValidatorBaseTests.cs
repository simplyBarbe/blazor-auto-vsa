using FluentAssertions;
using Server.Infrastructure.CRUD.Validators;
using Shared.Core;
using Shared.Core.CRUD;
using Xunit;

namespace Unit.Features.Common;

public class DeleteEntityCommandValidatorBaseTests
{
    [Fact]
    public void Command_with_Id_property_null_should_fail()
    {
        var validator = new TestDeleteCommandWithIdValidator();
        var command = new TestDeleteCommandWithNullableId { Id = null };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "The Id is required.");
    }

    [Fact]
    public void Command_with_Id_property_set_should_pass()
    {
        var validator = new TestDeleteCommandWithIdValidator();
        var command = new TestDeleteCommandWithNullableId { Id = 1 };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Command_with_empty_GetKeys_should_fail()
    {
        var validator = new TestDeleteCommandWithKeysValidator();
        var command = new TestDeleteCommandWithKeys { KeyPartA = 0, KeyPartB = 0 };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "The key is required.");
    }

    [Fact]
    public void Command_with_non_empty_GetKeys_should_pass()
    {
        var validator = new TestDeleteCommandWithKeysValidator();
        var command = new TestDeleteCommandWithKeys { KeyPartA = 1, KeyPartB = 2 };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    private sealed class TestDeleteCommandWithNullableId : IRequest<object?>, IEntityKeyProvider
    {
        public int? Id { get; set; }
        public object[] GetKeys() => Id.HasValue ? new object[] { Id.Value } : Array.Empty<object>();
    }

    private sealed class TestDeleteCommandWithKeys : IRequest<object?>, IEntityKeyProvider
    {
        public int KeyPartA { get; set; }
        public int KeyPartB { get; set; }
        public object[] GetKeys() => KeyPartA == 0 && KeyPartB == 0 ? Array.Empty<object>() : new object[] { KeyPartA, KeyPartB };
    }

    private sealed class TestDeleteCommandWithIdValidator : DeleteEntityCommandValidatorBase<TestDeleteCommandWithNullableId>
    {
    }

    private sealed class TestDeleteCommandWithKeysValidator : DeleteEntityCommandValidatorBase<TestDeleteCommandWithKeys>
    {
    }
}
