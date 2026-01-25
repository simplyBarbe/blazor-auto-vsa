using Shared.Core;
using Shared.Core.Validation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Dispatching;

/// <summary>
/// Implementation of IRequestSender that sends requests via HTTP.
/// Used when running in WebAssembly mode.
/// Handles validation errors returned from the server.
/// </summary>
public class HttpRequestSender : IRequestSender
{
    private readonly HttpClient _http;
    private readonly IRequestEndpointMapper _mapper;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpRequestSender(HttpClient http, IRequestEndpointMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var (url, method) = _mapper.GetEndpoint(request);

        if (method == HttpMethod.Get)
        {
            var getResult = await _http.GetFromJsonAsync<TResponse>(url, cancellationToken);
            return getResult ?? throw new InvalidOperationException($"Received null response from {url}");
        }

        // Cast to object so System.Text.Json uses the runtime type
        var response = await _http.PostAsJsonAsync(url, (object)request, cancellationToken);

        // Handle validation errors (400 Bad Request)
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            try
            {
                var validationErrors = JsonSerializer.Deserialize<ValidationErrorResponse>(content, JsonOptions);

                if (validationErrors?.Errors != null && validationErrors.Errors.Any())
                {
                    throw new ValidationException(validationErrors.Errors);
                }
            }
            catch (JsonException)
            {
                // Fall through to generic error handling below
            }

            // If it's not a validation error payload, surface the response body
            throw new HttpRequestException(
                $"Request failed with status {(int)response.StatusCode} {response.StatusCode}. Response: {content}");
        }

        response.EnsureSuccessStatusCode();

        var postResult = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        return postResult ?? throw new InvalidOperationException($"Received null response from {url}");
    }
}
