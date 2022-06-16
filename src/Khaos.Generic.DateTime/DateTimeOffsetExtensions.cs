namespace Khaos.Generic.DateTime;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset Trim(this DateTimeOffset dt, DateTimePrecision precision)
    {
        switch (precision)
        {
            case DateTimePrecision.Seconds:
                return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Offset);

            case DateTimePrecision.Minutes:
                return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Offset);

            case DateTimePrecision.Hours:
                return new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Offset);

            case DateTimePrecision.Days:
                return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Offset);

            case DateTimePrecision.Months:
                return new DateTimeOffset(dt.Year, dt.Month, 0, 0, 0, 0, 0, dt.Offset);

            case DateTimePrecision.Years:
                return new DateTimeOffset(dt.Year, dt.Offset);

            default:
                throw new ArgumentOutOfRangeException(nameof(precision));
        }
    }

    public static IEnumerable<DateTimeOffset> EnumerateAllHoursTo(
        this DateTimeOffset start, DateTimeOffset end,
        bool roundToNextFullHour = false, double nextHourRoundingThreshold = 0)
    {
        if (end < start)
        {
            throw new ArgumentException("Start date is greater than the end date.", nameof(end));
        }

        var delta = end - start;
        var roundHours = (int)Math.Truncate(delta.TotalHours);

        if (roundToNextFullHour)
        {
            var fraction = delta.TotalHours - roundHours;

            if (fraction > nextHourRoundingThreshold)
            {
                roundHours += 1;
            }
        }

        return Enumerable.Range(0, roundHours).Select(hour => start + TimeSpan.FromHours(hour));
    }
}