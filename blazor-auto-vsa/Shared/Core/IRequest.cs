namespace Shared.Core;

/// <summary>
/// Marker interface for requests that return a response of type TResponse.
/// </summary>
/// <typeparam name="TResponse">The type of response this request returns.</typeparam>
public interface IRequest<TResponse> { }
