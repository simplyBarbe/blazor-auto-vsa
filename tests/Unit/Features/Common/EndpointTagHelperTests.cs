using FluentAssertions;
using Server.Features.Auth;
using Server.Features.Products.Create;
using Server.Infrastructure.CRUD.Endpoints;
using Server.Infrastructure.Endpoints;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;
using Xunit;

namespace Unit.Features.Common;

public class EndpointTagHelperTests
{
    [Fact]
    public void GetFeatureTag_WhenNamespaceContainsFeatures_ReturnsSegmentAfterFeatures()
    {
        var tag = EndpointTagHelper.GetFeatureTag(typeof(CreateProductEndpoint));

        tag.Should().Be("Products");
    }

    [Fact]
    public void GetFeatureTag_WhenNamespaceContainsFeaturesAuth_ReturnsAuth()
    {
        var tag = EndpointTagHelper.GetFeatureTag(typeof(LoginHandler));

        tag.Should().Be("Auth");
    }

    [Fact]
    public void GetFeatureTag_WhenNoFeaturesInNamespace_ReturnsTypeNameWithoutGenericArity()
    {
        var tag = EndpointTagHelper.GetFeatureTag(typeof(CreateEntityEndpointBase<CreateProductCommand, ProductResponse>));

        tag.Should().Be("CreateEntityEndpointBase");
    }
}
