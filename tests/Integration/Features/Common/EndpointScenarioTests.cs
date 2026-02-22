using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Server.Infrastructure.CRUD.Endpoints;
using Server.Infrastructure.Endpoints;
using Shared.Core;
using Shared.Core.CRUD;

namespace Integration.Features.Common;

public class EndpointScenarioTests : IClassFixture<WebApplicationFactory<Server.Program>>
{
    private readonly WebApplicationFactory<Server.Program> _factory;

    public EndpointScenarioTests(WebApplicationFactory<Server.Program> factory)
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
}
