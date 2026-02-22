using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Client.Infrastructure.Auth;

public class AntiforgeryHandler : DelegatingHandler
{
    private readonly PersistentComponentState _state;
    private string? _token;

    public AntiforgeryHandler(PersistentComponentState state)
    {
        _state = state;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_token == null)
        {
            if (_state.TryTakeFromJson<string>("AntiforgeryToken", out var token))
            {
                _token = token;
            }
        }

        if (_token != null && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put || request.Method == HttpMethod.Delete))
        {
            request.Headers.Add("X-XSRF-TOKEN", _token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
