using Shared.Core;
using Shared.Features.Products.Create;
using Shared.Features.Products.Get;

namespace Client.Dispatching;

/// <summary>
/// Maps requests to their corresponding API endpoints.
/// </summary>
public class RequestEndpointMapper : IRequestEndpointMapper
{
    public (string Url, HttpMethod Method) GetEndpoint<TResponse>(IRequest<TResponse> request)
    {
        return request switch
        {
            GetProductQuery query => ($"/api/products/{query.Id}", HttpMethod.Get),
            CreateProductCommand => ("/api/products", HttpMethod.Post),
            _ => throw new InvalidOperationException($"Endpoint non mappato per {request.GetType().Name}")
        };
    }
}
