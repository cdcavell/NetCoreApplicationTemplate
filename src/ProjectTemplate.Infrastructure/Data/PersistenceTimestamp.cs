namespace ProjectTemplate.Infrastructure.Data;

internal static class PersistenceTimestamp
{
    internal const int Precision = 3;

    private const long _ticksPerMillisecond = TimeSpan.TicksPerMillisecond;

    internal static DateTime UtcNow()
    {
        return NormalizeUtc(DateTime.UtcNow);
    }

    internal static DateTime NormalizeUtc(DateTime value)
    {
        DateTime utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        long normalizedTicks = utcValue.Ticks - (utcValue.Ticks % _ticksPerMillisecond);

        return new DateTime(normalizedTicks, DateTimeKind.Utc);
    }

    internal static DateTimeOffset NormalizeUtc(DateTimeOffset value)
    {
        DateTimeOffset utcValue = value.ToUniversalTime();
        long normalizedTicks = utcValue.Ticks - (utcValue.Ticks % _ticksPerMillisecond);

        return new DateTimeOffset(normalizedTicks, TimeSpan.Zero);
    }
}
