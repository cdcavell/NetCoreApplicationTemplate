using ProjectTemplate.Infrastructure.Data;

namespace ProjectTemplate.Web.Tests;

public sealed class PersistenceTimestampTests
{
    [Fact]
    public void NormalizeUtc_UtcDateTime_TrimsToMillisecondPrecision()
    {
        DateTime value = new(2026, 5, 31, 12, 34, 56, 789, DateTimeKind.Utc);
        value = value.AddTicks(1_234);

        DateTime result = PersistenceTimestamp.NormalizeUtc(value);

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(0, result.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(TruncateToMillisecond(value).Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeUtc_AlreadyNormalizedUtcDateTime_ReturnsSameInstant()
    {
        DateTime value = new(2026, 5, 31, 12, 34, 56, 789, DateTimeKind.Utc);

        DateTime result = PersistenceTimestamp.NormalizeUtc(value);

        Assert.Equal(value, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void NormalizeUtc_UnspecifiedDateTime_TreatsValueAsUtc()
    {
        DateTime value = new(2026, 5, 31, 12, 34, 56, 789, DateTimeKind.Unspecified);
        value = value.AddTicks(9_999);

        DateTime result = PersistenceTimestamp.NormalizeUtc(value);

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(0, result.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(TruncateToMillisecond(DateTime.SpecifyKind(value, DateTimeKind.Utc)).Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeUtc_LocalDateTime_ConvertsToUtcAndTrimsPrecision()
    {
        DateTime value = new(2026, 5, 31, 12, 34, 56, 789, DateTimeKind.Local);
        value = value.AddTicks(4_321);

        DateTime result = PersistenceTimestamp.NormalizeUtc(value);

        DateTime expected = TruncateToMillisecond(value.ToUniversalTime());

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(0, result.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(expected.Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeUtc_DateTime_IsIdempotent()
    {
        DateTime value = new(2026, 5, 31, 12, 34, 56, 789, DateTimeKind.Utc);
        value = value.AddTicks(7_654);

        DateTime once = PersistenceTimestamp.NormalizeUtc(value);
        DateTime twice = PersistenceTimestamp.NormalizeUtc(once);

        Assert.Equal(once, twice);
        Assert.Equal(DateTimeKind.Utc, twice.Kind);
    }

    [Fact]
    public void NormalizeUtc_DateTimeOffset_ConvertsToUtcAndTrimsPrecision()
    {
        DateTimeOffset value = new(
            2026,
            5,
            31,
            12,
            34,
            56,
            789,
            TimeSpan.FromHours(-5));

        value = value.AddTicks(8_765);

        DateTimeOffset result = PersistenceTimestamp.NormalizeUtc(value);

        DateTimeOffset expected = TruncateToMillisecond(value.ToUniversalTime());

        Assert.Equal(TimeSpan.Zero, result.Offset);
        Assert.Equal(0, result.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(expected.Ticks, result.Ticks);
    }

    [Fact]
    public void NormalizeUtc_DateTimeOffset_IsIdempotent()
    {
        DateTimeOffset value = new(
            2026,
            5,
            31,
            12,
            34,
            56,
            789,
            TimeSpan.FromHours(2));

        value = value.AddTicks(3_210);

        DateTimeOffset once = PersistenceTimestamp.NormalizeUtc(value);
        DateTimeOffset twice = PersistenceTimestamp.NormalizeUtc(once);

        Assert.Equal(once, twice);
        Assert.Equal(TimeSpan.Zero, twice.Offset);
    }

    [Fact]
    public void NormalizeUtc_MinAndMaxUtcDateTimeValues_AreSupported()
    {
        var min = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        var max = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);

        DateTime normalizedMin = PersistenceTimestamp.NormalizeUtc(min);
        DateTime normalizedMax = PersistenceTimestamp.NormalizeUtc(max);

        Assert.Equal(DateTimeKind.Utc, normalizedMin.Kind);
        Assert.Equal(DateTimeKind.Utc, normalizedMax.Kind);
        Assert.Equal(min, normalizedMin);
        Assert.Equal(0, normalizedMax.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(TruncateToMillisecond(max).Ticks, normalizedMax.Ticks);
    }

    [Fact]
    public void UtcNow_ReturnsUtcTimestampWithMillisecondPrecision()
    {
        DateTime result = PersistenceTimestamp.UtcNow();

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(0, result.Ticks % TimeSpan.TicksPerMillisecond);
    }

    private static DateTime TruncateToMillisecond(DateTime value)
    {
        long normalizedTicks = value.Ticks - (value.Ticks % TimeSpan.TicksPerMillisecond);

        return new DateTime(normalizedTicks, value.Kind);
    }

    private static DateTimeOffset TruncateToMillisecond(DateTimeOffset value)
    {
        long normalizedTicks = value.Ticks - (value.Ticks % TimeSpan.TicksPerMillisecond);

        return new DateTimeOffset(normalizedTicks, value.Offset);
    }
}
