namespace Project.Application.Jobs;

public static class MinuteSchedule
{
    // Preserve the old */N minute cron in UTC: */45 means 00 and 45, not a rolling 45-minute interval.
    public static DateTime Next(DateTime utcNow, int step)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(step, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(step, 59);
        var hour = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc);
        var minute = (utcNow.Minute / step + 1) * step;
        return minute < 60 ? hour.AddMinutes(minute) : hour.AddHours(1);
    }
}
