namespace Server.Infrastructure.Common;

public interface ICurrentUserService
{
    Guid? GetCurrentUserId();
}
