namespace Server.Infrastructure.Endpoints;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
