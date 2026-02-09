using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Server.Infrastructure.Data.Converters;

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        // Convert to UTC when saving to database
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        // Ensure UTC when reading from database
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}