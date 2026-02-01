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

        HttpResponseMessage response;
        if (method == HttpMethod.Get)
        {
            response = await _http.GetAsync(url, cancellationToken);
        }
        else if (method == HttpMethod.Post)
        {
            response = await _http.PostAsJsonAsync(url, (object)request, cancellationToken);
        }
        else if (method == HttpMethod.Put)
        {
            response = await _http.PutAsJsonAsync(url, (object)request, cancellationToken);
        }
        else if (method == HttpMethod.Delete)
        {
            response = await _http.DeleteAsync(url, cancellationToken);
        }
        else if (method == HttpMethod.Patch)
        {
            response = await _http.PatchAsJsonAsync(url, (object)request, cancellationToken);
        }
        else
        {
            var message = new HttpRequestMessage(method, url);
            if (method != HttpMethod.Head && method != HttpMethod.Options)
            {
                message.Content = JsonContent.Create((object)request);
            }
            response = await _http.SendAsync(message, cancellationToken);
        }

        // Handle validation errors (400 Bad Request)
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Server returned BadRequest: {content}");

            try
            {
                var validationErrors = JsonSerializer.Deserialize<ValidationErrorResponse>(content, JsonOptions);

                if (validationErrors?.Errors != null && validationErrors.Errors.Any())
                {
                    Console.WriteLine($"Throwing ValidationException with {validationErrors.Errors.Count} errors");
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

        if (typeof(TResponse) == typeof(object) || response.StatusCode == HttpStatusCode.NoContent)
        {
            return default!;
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        return result ?? throw new InvalidOperationException($"Received null response from {url}");
    }
}
