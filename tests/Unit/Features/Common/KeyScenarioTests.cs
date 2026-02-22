using Shared.Core.CRUD;
using FluentAssertions;
using Xunit;
using Moq;
using Shared.Core;
using Server.Infrastructure.Data.Contracts;
using AutoMapper;
using Server.Infrastructure.CRUD.Handlers;

using Shared.Core.Exceptions;

namespace Unit.Features.Common;

public class KeyScenarioTests
{
    private readonly KeyExtractor _keyExtractor = KeyExtractor.Default;

    #region Scenarios Models

    public class GetByIdIntQuery : IRequest<string>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public object[] GetKeys() => [Id];
    }

    public class GetByCodeIntQuery : IRequest<string>, IEntityKeyProvider
    {
        public int Code { get; set; }
        public object[] GetKeys() => [Code];
    }

    public class GetByCodeGuidQuery : IRequest<string>, IEntityKeyProvider
    {
        public Guid Code { get; set; }
        public object[] GetKeys() => [Code];
    }

    public record CompositeKey(int UserId, int GroupId);

    public class GetByCompositeKeyQuery : IRequest<string>, IEntityKeyProvider
    {
        public required CompositeKey Key { get; set; }
        public object[] GetKeys() => [Key.UserId, Key.GroupId];
    }

    public class DummyEntity { }

    #endregion

    [Fact]
    public void KeyExtractor_WhenCommandNull_ThrowsArgumentNullException()
    {
        var act = () => _keyExtractor.GetKeyValues(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("commandOrQuery");
    }

    [Fact]
    public void KeyExtractor_ShouldExtract_IntId()
    {
        var query = new GetByIdIntQuery { Id = 123 };
        var keys = _keyExtractor.GetKeyValues(query);
        keys.Should().ContainSingle().Which.Should().Be(123);
    }

    [Fact]
    public void KeyExtractor_ShouldExtract_IntCode()
    {
        var query = new GetByCodeIntQuery { Code = 456 };
        var keys = _keyExtractor.GetKeyValues(query);
        keys.Should().ContainSingle().Which.Should().Be(456);
    }

    [Fact]
    public void KeyExtractor_ShouldExtract_GuidCode()
    {
        var guid = Guid.NewGuid();
        var query = new GetByCodeGuidQuery { Code = guid };
        var keys = _keyExtractor.GetKeyValues(query);
        keys.Should().ContainSingle().Which.Should().Be(guid);
    }

    [Fact]
    public void KeyExtractor_ShouldExtract_CompositeKey_Record()
    {
        var query = new GetByCompositeKeyQuery { Key = new CompositeKey(1, 2) };
        var keys = _keyExtractor.GetKeyValues(query);
        keys.Should().HaveCount(2);
        keys[0].Should().Be(1);
        keys[1].Should().Be(2);
    }

    [Fact]
    public void KeyExtractor_ShouldExtract_CompositeKey_Tuple()
    {
        var query = new GetByTupleQuery { Key = (1, 2) };
        var keys = _keyExtractor.GetKeyValues(query);
        keys.Should().HaveCount(2);
        keys[0].Should().Be(1);
        keys[1].Should().Be(2);
    }

    public class GetByTupleQuery : IRequest<string>, IEntityKeyProvider
    {
        public (int UserId, int GroupId) Key { get; set; }
        public object[] GetKeys() => [Key.Item1, Key.Item2];
    }

    [Fact]
    public async Task GetEntityHandlerBase_ShouldUseExtractedKeys()
    {
        // Arrange
        var mockUow = new Mock<IUnitOfWork>();
        var mockRepo = new Mock<IReadRepository<DummyEntity>>();
        var mockMapper = new Mock<IMapper>();

        var query = new GetByCompositeKeyQuery { Key = new CompositeKey(10, 20) };
        var entity = new DummyEntity();

        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockRepo.Object);
        mockRepo.Setup(x => x.GetByKeyAsync(It.IsAny<object[]>())).ReturnsAsync(entity);
        mockMapper.Setup(x => x.Map<string>(entity)).Returns("Success");

        var handler = new TestGetHandler(mockUow.Object, mockMapper.Object);

        // Act
        var result = await handler.Handle(query);

        // Assert
        result.Should().Be("Success");
        mockRepo.Verify(x => x.GetByKeyAsync(It.Is<object[]>(k => (int)k[0] == 10 && (int)k[1] == 20)), Times.Once);
    }

    private class TestGetHandler : GetEntityHandlerBase<DummyEntity, GetByCompositeKeyQuery, string>
    {
        public TestGetHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
    }

    [Fact]
    public void EntityNotFoundException_ShouldFormatSimpleKey()
    {
        var ex = new EntityNotFoundException("Product", 123);
        ex.Message.Should().Be("Entity 'Product' with identifier '123' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_ShouldFormatCompositeKeyArray()
    {
        var ex = new EntityNotFoundException("Product", new object[] { 10, "ABC" });
        ex.Message.Should().Be("Entity 'Product' with identifier '[10, ABC]' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_ShouldFormatCompositeKeyList()
    {
        var ex = new EntityNotFoundException("Product", new List<object> { 10, 20 });
        ex.Message.Should().Be("Entity 'Product' with identifier '[10, 20]' was not found.");
    }
}
