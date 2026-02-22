namespace Server.Infrastructure.Common;

public interface IDateTimeProvider
{
    DateTime GetUtcNow();
}
