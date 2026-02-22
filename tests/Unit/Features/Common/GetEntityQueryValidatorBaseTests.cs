using FluentAssertions;
using Server.Infrastructure.CRUD.Validators;
using Shared.Core;
using Shared.Core.CRUD;
using Xunit;

namespace Unit.Features.Common;

public class GetEntityQueryValidatorBaseTests
{
    [Fact]
    public void Query_with_empty_GetKeys_should_fail()
    {
        var validator = new TestGetEntityQueryValidator();
        var query = new TestCompositeKeyQuery { KeyPartA = 0, KeyPartB = 0 }; // empty keys

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "The key is required.");
    }

    [Fact]
    public void Query_with_non_empty_GetKeys_should_pass()
    {
        var validator = new TestGetEntityQueryValidator();
        var query = new TestCompositeKeyQuery { KeyPartA = 1, KeyPartB = 2 };

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_with_Id_property_nullable_set_should_pass()
    {
        var validator = new TestGetByIdQueryValidator();
        var query = new TestQueryWithNullableId { Id = 1 };

        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Query_with_Id_property_nullable_null_should_fail()
    {
        var validator = new TestGetByIdQueryValidator();
        var query = new TestQueryWithNullableId { Id = null };

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "The Id is required.");
    }

    private sealed class TestCompositeKeyQuery : IRequest<object>, IEntityKeyProvider
    {
        public int KeyPartA { get; set; }
        public int KeyPartB { get; set; }
        public object[] GetKeys() => KeyPartA == 0 && KeyPartB == 0 ? Array.Empty<object>() : new object[] { KeyPartA, KeyPartB };
    }

    private sealed class TestGetEntityQueryValidator : GetEntityQueryValidatorBase<TestCompositeKeyQuery>
    {
    }

    private sealed class TestQueryWithNullableId : IRequest<object>, IEntityKeyProvider
    {
        public int? Id { get; set; }
        public object[] GetKeys() => Id.HasValue ? new object[] { Id.Value } : Array.Empty<object>();
    }

    private sealed class TestGetByIdQueryValidator : GetEntityQueryValidatorBase<TestQueryWithNullableId>
    {
    }
}
