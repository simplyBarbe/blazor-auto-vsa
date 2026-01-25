namespace Shared.Core.CRUD;

/// <summary>
/// Base command for creating an entity.
/// </summary>
/// <typeparam name="TResponse">The type of response returned after creation.</typeparam>
public abstract class CreateEntityCommand<TResponse> : IRequest<TResponse>
{
}
