using Shared.Core;

namespace Client.Dispatching;

public interface IRequestEndpointMapper
{
    (string Url, HttpMethod Method) GetEndpoint<TResponse>(IRequest<TResponse> request);
}
