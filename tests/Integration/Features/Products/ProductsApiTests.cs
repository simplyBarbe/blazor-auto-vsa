using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Integration.Infrastructure;
using Shared.Core;
using Shared.Features.Products.Create;
using Shared.Features.Products.Responses;
using Shared.Features.Products.Update;
using Xunit;

namespace Integration.Features.Products;

[Collection(Integration.Infrastructure.IntegrationCollection.Name)]
public class ProductsApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_should_return_201_and_location_and_body()
    {
        var command = new CreateProductCommand("Integration Product", 19.99m);

        var response = await _client.PostAsJsonAsync("/api/products", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/products/");

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Integration Product");
        product.Price.Should().Be(19.99m);
        product.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProduct_after_create_should_return_200_and_same_data()
    {
        var command = new CreateProductCommand("Get Test Product", 29.99m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", command);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/products/{created!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(created.Id);
        product.Name.Should().Be("Get Test Product");
        product.Price.Should().Be(29.99m);
    }

    [Fact]
    public async Task GetProduct_not_found_should_return_404()
    {
        var response = await _client.GetAsync("/api/products/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListProducts_should_return_paged_result()
    {
        var response = await _client.GetAsync("/api/products?PageNumber=1&PageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task UpdateProduct_should_return_200_and_updated_data()
    {
        var createCommand = new CreateProductCommand("To Update", 10m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        created.Should().NotBeNull();

        var updateCommand = new UpdateProductCommand(created!.Id, "Updated Name", 99.99m);
        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updateCommand);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductResponse>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.Price.Should().Be(99.99m);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        var getProduct = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        getProduct!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteProduct_should_return_204_and_then_get_404()
    {
        var createCommand = new CreateProductCommand("To Delete", 5m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createCommand);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        created.Should().NotBeNull();

        var deleteResponse = await _client.DeleteAsync($"/api/products/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_with_invalid_data_should_return_400()
    {
        var command = new CreateProductCommand("", -1m); // empty name, negative price

        var response = await _client.PostAsJsonAsync("/api/products", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
