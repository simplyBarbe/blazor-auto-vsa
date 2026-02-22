using Server.Infrastructure.Common;

namespace Server.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;
}
