using Client.Dispatching;
using Client.Features.Products;
using FluentAssertions;
using Shared.Core;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using Xunit;

namespace Unit.Features.Dispatching;

public class RequestEndpointMapperTests
{
    private static RequestEndpointMapper CreateMapper()
    {
        var definitions = new IRouteDefinition[] { new ProductRoutes() };
        return new RequestEndpointMapper(definitions);
    }

    [Fact]
    public void GetEndpoint_GetProductQuery_should_substitute_Id_and_return_Get()
    {
        var mapper = CreateMapper();
        var request = new GetProductQuery(42);

        var (url, method) = mapper.GetEndpoint<ProductResponse>(request);

        url.Should().Contain("42");
        url.Should().Contain("/api/products/");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_ListProductQuery_should_append_query_string_for_GET()
    {
        var mapper = CreateMapper();
        var request = new ListProductQuery { PageNumber = 2, PageSize = 5 };

        var (url, method) = mapper.GetEndpoint<PagedResult<ProductResponse>>(request);

        url.Should().Contain("/api/products");
        url.Should().Contain("PageNumber=2");
        url.Should().Contain("PageSize=5");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_CreateProductCommand_should_return_post_and_base_url()
    {
        var mapper = CreateMapper();
        var request = new CreateProductCommand("Test", 10m);

        var (url, method) = mapper.GetEndpoint<ProductResponse>(request);

        url.Should().Be("/api/products");
        method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public void GetEndpoint_unmapped_request_type_should_throw()
    {
        var mapper = CreateMapper();
        var request = new UnmappedRequest();

        var act = () => mapper.GetEndpoint<object?>(request);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No route mapped*UnmappedRequest*");
    }

    private sealed class UnmappedRequest : IRequest<object?>
    {
    }
}
