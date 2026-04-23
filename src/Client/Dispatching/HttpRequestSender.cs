using Shared.Core;
using Shared.Core.Validation;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Dispatching;

public class HttpRequestSender : IRequestSender
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _http;
    private readonly IRequestEndpointMapper _mapper;

    public HttpRequestSender(HttpClient http, IRequestEndpointMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var (url, method) = _mapper.GetEndpoint(request);

        var response = method.Method switch
        {
            "GET" => await _http.GetAsync(url, cancellationToken),
            "POST" => await _http.PostAsJsonAsync(url, (object)request, cancellationToken),
            "PUT" => await _http.PutAsJsonAsync(url, (object)request, cancellationToken),
            "DELETE" => await _http.DeleteAsync(url, cancellationToken),
            "PATCH" => await _http.PatchAsJsonAsync(url, (object)request, cancellationToken),
            _ => await SendFallbackAsync(method, url, request, cancellationToken)
        };

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            TryThrowValidation(content);
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

    private async Task<HttpResponseMessage> SendFallbackAsync(HttpMethod method, string url, object request, CancellationToken ct)
    {
        var message = new HttpRequestMessage(method, url);
        if (method != HttpMethod.Head && method != HttpMethod.Options)
        {
            message.Content = JsonContent.Create(request);
        }
        return await _http.SendAsync(message, ct);
    }

    private static void TryThrowValidation(string content)
    {
        try
        {
            var validationErrors = JsonSerializer.Deserialize<ValidationErrorResponse>(content, JsonOptions);
            if (validationErrors?.Errors != null && validationErrors.Errors.Any())
            {
                throw new ValidationException(validationErrors.Errors);
            }
        }
        catch (JsonException) { }
    }
}
