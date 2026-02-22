using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Server.Infrastructure.CRUD.Endpoints;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;

namespace Integration.Features.Common;

[Collection(Integration.Infrastructure.IntegrationCollection.Name)]
public class EndpointScenarioTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public EndpointScenarioTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Mock Models

    public class TestResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class GetByIdQuery : IRequest<TestResponse>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public object[] GetKeys() => [Id];
    }

    public class GetByGuidQuery : IRequest<TestResponse>, IEntityKeyProvider
    {
        public Guid Code { get; set; }
        public object[] GetKeys() => [Code];
    }

    public record CompositeKey(int UserId, int GroupId);

    public class GetByCompositeKeyQuery : IRequest<TestResponse>, IEntityKeyProvider
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public object[] GetKeys() => [UserId, GroupId];
    }

    public class CreateTestCommand : IRequest<TestResponse>
    {
        public string Name { get; set; } = "";
    }

    public class ListTestQuery : IRequest<PagedResult<TestResponse>>, IPageableQuery
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class UpdateTestCommand : IRequest<TestResponse>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public object[] GetKeys() => [Id];
    }

    public class DeleteTestCommand : IRequest<object?>, IEntityKeyProvider
    {
        public int Id { get; set; }
        public object[] GetKeys() => [Id];
    }

    #endregion

    #region Mock Endpoints

    public class TestGetByIdEndpoint : GetEntityEndpointBase<int, GetByIdQuery, TestResponse>
    {
        protected override string GetRoute() => "/api/test-int/{key:int}";
        protected override GetByIdQuery CreateQuery(int key) => new GetByIdQuery { Id = key };
    }

    public class TestGetByGuidEndpoint : GetEntityEndpointBase<Guid, GetByGuidQuery, TestResponse>
    {
        protected override string GetRoute() => "/api/test-guid/{key:guid}";
        protected override GetByGuidQuery CreateQuery(Guid key) => new GetByGuidQuery { Code = key };
    }

    public class TestGetByCompositeEndpoint : GetEntityEndpointBase<CompositeKey, GetByCompositeKeyQuery, TestResponse>
    {
        protected override string GetRoute() => "/api/test-composite/{UserId:int}/{GroupId:int}";
        protected override GetByCompositeKeyQuery CreateQuery(CompositeKey key) => new GetByCompositeKeyQuery { UserId = key.UserId, GroupId = key.GroupId };

        protected override async Task<IResult> HandleAsync(
            [AsParameters] CompositeKey key,
            IRequestSender sender,
            CancellationToken cancellationToken = default)
        {
            return await base.HandleAsync(key, sender, cancellationToken);
        }
    }

    public class TestCreateEndpoint : CreateEntityEndpointBase<CreateTestCommand, TestResponse>
    {
        protected override string GetRoute() => "/api/test-create";
        protected override string GetCreatedLocation(object result) => "/api/test-create/1";
    }

    public class TestListEndpoint : ListEntityEndpointBase<ListTestQuery, TestResponse>
    {
        protected override string GetRoute() => "/api/test-list";
    }

    public class TestUpdateEndpoint : UpdateEntityEndpointBase<int, UpdateTestCommand, TestResponse>
    {
        protected override string GetRoute() => "/api/test-update/{key:int}";
    }

    public class TestDeleteEndpoint : DeleteEntityEndpointBase<int, DeleteTestCommand>
    {
        protected override string GetRoute() => "/api/test-delete/{key:int}";
        protected override DeleteTestCommand CreateCommand(int key) => new DeleteTestCommand { Id = key };
    }

    #endregion

    [Fact]
    public async Task Get_WithIntKey_ShouldBindAndReturnOk()
    {
        // Arrange
        var mockSender = new Mock<IRequestSender>();
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse { Message = "Success" });

        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockSender.Object);
            });
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new TestGetByIdEndpoint().Map(endpoints);
                });
            });
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/test-int/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TestResponse>();
        content!.Message.Should().Be("Success");
        mockSender.Verify(x => x.Send<TestResponse>(It.Is<GetByIdQuery>(q => q.Id == 123), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithGuidKey_ShouldBindAndReturnOk()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var mockSender = new Mock<IRequestSender>();
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<GetByGuidQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse { Message = "Success" });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockSender.Object);
            });
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new TestGetByGuidEndpoint().Map(endpoints);
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/test-guid/{guid}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TestResponse>();
        content!.Message.Should().Be("Success");
        mockSender.Verify(x => x.Send<TestResponse>(It.Is<GetByGuidQuery>(q => q.Code == guid), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithCompositeKey_ShouldBindAndReturnOk()
    {
        // Arrange
        var mockSender = new Mock<IRequestSender>();
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<GetByCompositeKeyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse { Message = "Success" });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockSender.Object);
            });
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new TestGetByCompositeEndpoint().Map(endpoints);
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/test-composite/10/20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TestResponse>();
        content!.Message.Should().Be("Success");
        mockSender.Verify(x => x.Send<TestResponse>(It.Is<GetByCompositeKeyQuery>(q => q.UserId == 10 && q.GroupId == 20), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithNotFound_ShouldReturn404()
    {
        // Arrange
        var mockSender = new Mock<IRequestSender>();
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Shared.Core.Exceptions.EntityNotFoundException("Test", 123));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockSender.Object);
            });
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new TestGetByIdEndpoint().Map(endpoints);
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/test-int/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_WithValidationError_ShouldReturn400()
    {
        // Arrange
        var mockSender = new Mock<IRequestSender>();
        var errors = new List<Shared.Core.Validation.ValidationError>
        {
            new() { PropertyName = "Id", ErrorMessage = "Invalid Id" }
        };
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<GetByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Shared.Core.Validation.ValidationException(errors));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockSender.Object);
            });
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    new TestGetByIdEndpoint().Map(endpoints);
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/test-int/123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<Shared.Core.Validation.ValidationErrorResponse>();
        content!.Errors.Should().HaveCount(1);
        content.Errors[0].ErrorMessage.Should().Be("Invalid Id");
    }

    [Fact]
    public async Task Create_ShouldReturn201AndLocationAndBody()
    {
        var mockSender = new Mock<IRequestSender>();
        var created = new TestResponse { Message = "Created" };
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<CreateTestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services => services.AddScoped(_ => mockSender.Object));
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => new TestCreateEndpoint().Map(endpoints));
            });
        }).CreateClient();

        var response = await client.PostAsJsonAsync("/api/test-create", new CreateTestCommand { Name = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/test-create");
        var body = await response.Content.ReadFromJsonAsync<TestResponse>();
        body!.Message.Should().Be("Created");
    }

    [Fact]
    public async Task List_ShouldBindQueryAndReturn200WithPagedResult()
    {
        var mockSender = new Mock<IRequestSender>();
        var paged = new PagedResult<TestResponse>
        {
            Items = [new TestResponse { Message = "A" }],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };
        mockSender.Setup(x => x.Send<PagedResult<TestResponse>>(It.IsAny<ListTestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services => services.AddScoped(_ => mockSender.Object));
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => new TestListEndpoint().Map(endpoints));
            });
        }).CreateClient();

        var response = await client.GetAsync("/api/test-list?PageNumber=1&PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<TestResponse>>();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Message.Should().Be("A");
        mockSender.Verify(x => x.Send<PagedResult<TestResponse>>(It.Is<ListTestQuery>(q => q.PageNumber == 1 && q.PageSize == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldBindKeyAndBodyAndReturn200()
    {
        var mockSender = new Mock<IRequestSender>();
        var updated = new TestResponse { Message = "Updated" };
        mockSender.Setup(x => x.Send<TestResponse>(It.IsAny<UpdateTestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services => services.AddScoped(_ => mockSender.Object));
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => new TestUpdateEndpoint().Map(endpoints));
            });
        }).CreateClient();

        var response = await client.PutAsJsonAsync("/api/test-update/42", new UpdateTestCommand { Id = 42, Name = "Updated" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TestResponse>();
        body!.Message.Should().Be("Updated");
        mockSender.Verify(x => x.Send<TestResponse>(It.Is<UpdateTestCommand>(c => c.Id == 42 && c.Name == "Updated"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldBindKeyAndReturn204()
    {
        var mockSender = new Mock<IRequestSender>();
        mockSender.Setup(x => x.Send<object?>(It.IsAny<DeleteTestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as object);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services => services.AddScoped(_ => mockSender.Object));
            builder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => new TestDeleteEndpoint().Map(endpoints));
            });
        }).CreateClient();

        var response = await client.DeleteAsync("/api/test-delete/99");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        mockSender.Verify(x => x.Send<object?>(It.Is<DeleteTestCommand>(c => c.Id == 99), It.IsAny<CancellationToken>()), Times.Once);
    }
}
