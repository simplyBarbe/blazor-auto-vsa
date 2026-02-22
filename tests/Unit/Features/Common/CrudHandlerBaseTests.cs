using AutoMapper;
using FluentAssertions;
using Moq;
using Server.Infrastructure.CRUD.Handlers;
using Server.Infrastructure.Data.Contracts;
using Shared.Core;
using Shared.Core.CRUD;
using Shared.Core.Exceptions;
using Xunit;

namespace Unit.Features.Common;

public class CrudHandlerBaseTests
{
    public sealed class DummyEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class CreateCommand : IRequest<CreateResponse>
    {
        public string Name { get; set; } = "";
    }

    private sealed class CreateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class UpdateCommand : IRequest<UpdateResponse>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public object[] GetKeys() => new object[] { Id };
    }

    private sealed class UpdateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class DeleteCommand : IRequest<object?>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public object[] GetKeys() => new object[] { Id };
    }

    private sealed class ListQuery : IRequest<PagedResult<ListItemResponse>>, IPageableQuery
    {
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
    }

    private sealed class ListItemResponse
    {
        public int Id { get; set; }
    }

    private sealed class TestCreateHandler : CreateEntityHandlerBase<DummyEntity, CreateCommand, CreateResponse>
    {
        public TestCreateHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }
    }

    private sealed class TestUpdateHandler : UpdateEntityHandlerBase<DummyEntity, UpdateCommand, UpdateResponse>
    {
        public TestUpdateHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }
    }

    private sealed class TestDeleteHandler : DeleteEntityHandlerBase<DummyEntity, DeleteCommand>
    {
        public TestDeleteHandler(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    private sealed class TestListHandler : ListEntityHandlerBase<DummyEntity, ListQuery, ListItemResponse>
    {
        public TestListHandler(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }
        protected override QueryFilter<DummyEntity> BuildQueryFilter(ListQuery query)
        {
            return new QueryFilter<DummyEntity>
            {
                Skip = ((query.PageNumber ?? 1) - 1) * (query.PageSize ?? 10),
                Take = query.PageSize ?? 10
            };
        }
    }

    [Fact]
    public async Task CreateEntityHandlerBase_should_call_AddAsync_and_SaveChanges_and_return_mapped_response()
    {
        var mockWriteRepo = new Mock<IWriteRepository<DummyEntity>>();
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.WriteRepository<DummyEntity>()).Returns(mockWriteRepo.Object);

        var entity = new DummyEntity { Id = 1, Name = "Test" };
        var response = new CreateResponse { Id = 1, Name = "Test" };
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(x => x.Map<DummyEntity>(It.IsAny<CreateCommand>())).Returns(entity);
        mockMapper.Setup(x => x.Map<CreateResponse>(entity)).Returns(response);

        var handler = new TestCreateHandler(mockUow.Object, mockMapper.Object);
        var command = new CreateCommand { Name = "Test" };

        var result = await handler.Handle(command);

        result.Should().BeSameAs(response);
        mockWriteRepo.Verify(x => x.AddAsync(It.Is<DummyEntity>(e => e.Name == "Test"), It.IsAny<CancellationToken>()), Times.Once);
        mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEntityHandlerBase_when_entity_not_found_should_throw_EntityNotFoundException()
    {
        var mockReadRepo = new Mock<IReadRepository<DummyEntity>>();
        mockReadRepo.Setup(x => x.GetByKeyAsync(It.IsAny<object[]>())).ReturnsAsync((DummyEntity?)null);
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockReadRepo.Object);

        var mockMapper = new Mock<IMapper>();
        var handler = new TestUpdateHandler(mockUow.Object, mockMapper.Object);
        var command = new UpdateCommand { Id = 999, Name = "X" };

        var act = () => handler.Handle(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateEntityHandlerBase_when_entity_found_should_Update_and_SaveChanges()
    {
        var entity = new DummyEntity { Id = 1, Name = "Old" };
        var mockReadRepo = new Mock<IReadRepository<DummyEntity>>();
        mockReadRepo.Setup(x => x.GetByKeyAsync(1)).ReturnsAsync(entity);
        var mockWriteRepo = new Mock<IWriteRepository<DummyEntity>>();
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockReadRepo.Object);
        mockUow.Setup(x => x.WriteRepository<DummyEntity>()).Returns(mockWriteRepo.Object);

        var response = new UpdateResponse { Id = 1, Name = "New" };
        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(x => x.Map<UpdateResponse>(entity)).Returns(response);

        var handler = new TestUpdateHandler(mockUow.Object, mockMapper.Object);
        var command = new UpdateCommand { Id = 1, Name = "New" };

        var result = await handler.Handle(command);

        result.Should().BeSameAs(response);
        mockWriteRepo.Verify(x => x.Update(It.Is<DummyEntity>(e => e.Id == 1)), Times.Once);
        mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEntityHandlerBase_when_entity_not_found_should_throw_EntityNotFoundException()
    {
        var mockReadRepo = new Mock<IReadRepository<DummyEntity>>();
        mockReadRepo.Setup(x => x.GetByKeyAsync(It.IsAny<object[]>())).ReturnsAsync((DummyEntity?)null);
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockReadRepo.Object);

        var handler = new TestDeleteHandler(mockUow.Object);
        var command = new DeleteCommand { Id = 999 };

        var act = () => handler.Handle(command);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteEntityHandlerBase_when_entity_found_should_Delete_and_SaveChanges()
    {
        var entity = new DummyEntity { Id = 1, Name = "X" };
        var mockReadRepo = new Mock<IReadRepository<DummyEntity>>();
        mockReadRepo.Setup(x => x.GetByKeyAsync(1)).ReturnsAsync(entity);
        var mockWriteRepo = new Mock<IWriteRepository<DummyEntity>>();
        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockReadRepo.Object);
        mockUow.Setup(x => x.WriteRepository<DummyEntity>()).Returns(mockWriteRepo.Object);

        var handler = new TestDeleteHandler(mockUow.Object);
        var command = new DeleteCommand { Id = 1 };

        var result = await handler.Handle(command);

        result.Should().BeNull();
        mockWriteRepo.Verify(x => x.Delete(It.Is<DummyEntity>(e => e.Id == 1)), Times.Once);
        mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListEntityHandlerBase_should_call_GetAsync_and_CountAsync_and_return_PagedResult()
    {
        var items = new List<ListItemResponse> { new() { Id = 1 } };
        var mockReadRepo = new Mock<IReadRepository<DummyEntity>>();
        mockReadRepo.Setup(x => x.GetAsync<ListItemResponse>(It.IsAny<QueryFilter<DummyEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(items);
        mockReadRepo.Setup(x => x.CountAsync(It.IsAny<QueryFilter<DummyEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockUow = new Mock<IUnitOfWork>();
        mockUow.Setup(x => x.ReadRepository<DummyEntity>()).Returns(mockReadRepo.Object);

        var mockMapper = new Mock<IMapper>();
        var handler = new TestListHandler(mockUow.Object, mockMapper.Object);
        var query = new ListQuery();

        var result = await handler.Handle(query);

        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
