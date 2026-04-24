using Client.Dispatching;
using Client.Features.Categories;
using Client.Features.ProductGroups;
using Client.Features.Products;
using FluentAssertions;
using Shared.Core;
using Shared.Features.Categories.List;
using Shared.Features.Categories.Responses;
using Shared.Features.ProductGroups.List;
using Shared.Features.ProductGroups.Responses;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;
using Shared.Features.Products.List;
using Shared.Features.Products.Responses;
using System.Globalization;
using Xunit;
namespace Unit.Features.Dispatching;

public class RequestEndpointMapperTests
{
    private static RequestEndpointMapper CreateMapper()
        => new(new IRouteDefinition[]
        {
            new ProductRoutes(),
            new CategoryRoutes(),
            new GroupRoutes()
        });
    [Fact]
    public void GetEndpoint_GetProductQuery_should_substitute_Id_and_return_Get()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<ProductResponse>(new GetProductQuery(42));

        url.Should().Contain("42");
        url.Should().Contain("/api/products/");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_ListProductQuery_should_append_query_string_for_GET()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<PagedResult<ProductResponse>>(
            new ListProductQuery { PageNumber = 2, PageSize = 5 });

        url.Should().Contain("/api/products");
        url.Should().Contain("PageNumber=2");
        url.Should().Contain("PageSize=5");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_ListProductQuery_should_include_category_and_group_filters()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<PagedResult<ProductResponse>>(
            new ListProductQuery { PageNumber = 1, PageSize = 10, CategoryId = 3, GroupId = 7 });

        url.Should().Contain("/api/products");
        url.Should().Contain("CategoryId=3");
        url.Should().Contain("GroupId=7");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_ListProductGroupQuery_should_include_category_filter()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<PagedResult<ProductGroupResponse>>(
            new ListProductGroupQuery { PageNumber = 1, PageSize = 20, CategoryId = 5 });

        url.Should().Contain("/api/groups");
        url.Should().Contain("CategoryId=5");
        method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public void GetEndpoint_ListCategoryQuery_should_resolve()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<PagedResult<CategoryResponse>>(
            new ListCategoryQuery { PageNumber = 1, PageSize = 10 });

        url.Should().Contain("/api/categories");
        method.Should().Be(HttpMethod.Get);
    }
    [Fact]
    public void GetEndpoint_CreateProductCommand_should_return_post_and_base_url()
    {
        var mapper = CreateMapper();

        var (url, method) = mapper.GetEndpoint<ProductResponse>(new CreateProductCommand(1, "Test", 10m));

        url.Should().Be("/api/products");
        method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public void GetEndpoint_unmapped_request_type_should_throw()
    {
        var mapper = CreateMapper();

        var act = () => mapper.GetEndpoint<object?>(new UnmappedRequest());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No route mapped*UnmappedRequest*");
    }

    [Fact]
    public void GetEndpoint_GetRequest_should_serialize_route_and_query_values_with_invariant_culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var italianCulture = new CultureInfo("it-IT");
            CultureInfo.CurrentCulture = italianCulture;
            CultureInfo.CurrentUICulture = italianCulture;

            var mapper = new RequestEndpointMapper(new IRouteDefinition[]
            {
                new ProductRoutes(),
                new CategoryRoutes(),
                new GroupRoutes(),
                new CultureAwareRoutes()
            });

            var request = new CultureAwareQuery(12.34m, new DateTime(2026, 01, 02, 15, 16, 17, DateTimeKind.Utc), 8.9d);

            var (url, method) = mapper.GetEndpoint<object?>(request);

            url.Should().StartWith("/api/culture-aware/12.34/01%2F02%2F2026%2015%3A16%3A17");
            url.Should().Contain("Ratio=8.9");
            url.Should().NotContain("Amount=");
            method.Should().Be(HttpMethod.Get);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private sealed class UnmappedRequest : IRequest<object?> { }

    private sealed class CultureAwareQuery(decimal Amount, DateTime When, double Ratio) : IRequest<object?>
    {
        public decimal Amount { get; } = Amount;
        public DateTime When { get; } = When;
        public double Ratio { get; } = Ratio;
    }

    private sealed class CultureAwareRoutes : IRouteDefinition
    {
        public void Define(RequestEndpointMapper mapper)
        {
            mapper.Map<CultureAwareQuery>("/api/culture-aware/{Amount}/{When}", HttpMethod.Get);
        }
    }
}
