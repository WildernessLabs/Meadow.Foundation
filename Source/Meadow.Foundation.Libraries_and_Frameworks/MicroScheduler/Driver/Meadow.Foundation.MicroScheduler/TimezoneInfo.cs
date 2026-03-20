using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents timezone and daylight saving time information for schedule collections.
/// All schedule times remain in UTC, and this information is used for display and editing purposes.
/// </summary>
public class TimezoneInfo
{
    /// <summary>
    /// Gets or sets the timezone name (e.g., "America/New_York", "Europe/London").
    /// </summary>
    public string TimezoneName { get; set; } = "UTC";

    /// <summary>
    /// Gets or sets the current timezone offset from UTC in hours.
    /// This is the standard time offset, not including DST.
    /// </summary>
    public double UtcOffsetHours { get; set; } = 0;

    /// <summary>
    /// Gets or sets the daylight saving time information.
    /// If null, no DST is observed.
    /// </summary>
    public DaylightSavingTimeInfo? DaylightSavingTime { get; set; }

    /// <summary>
    /// Determines if daylight saving time is currently active based on the given UTC date.
    /// </summary>
    /// <param name="utcDate">The UTC date to check.</param>
    /// <returns>True if DST is active on the given date, false otherwise.</returns>
    public bool IsDaylightSavingTimeActive(DateTimeOffset utcDate)
    {
        if (DaylightSavingTime == null) return false;

        var localDate = utcDate.AddHours(UtcOffsetHours);
        var year = localDate.Year;

        var startDate = DaylightSavingTime.GetStartDate(year);
        var endDate = DaylightSavingTime.GetEndDate(year);

        // Handle DST periods that cross year boundaries
        if (startDate > endDate)
        {
            return localDate >= startDate || localDate < endDate;
        }
        else
        {
            return localDate >= startDate && localDate < endDate;
        }
    }

    /// <summary>
    /// Gets the current total offset from UTC including DST if active.
    /// </summary>
    /// <param name="utcDate">The UTC date to calculate offset for.</param>
    /// <returns>The total offset in hours from UTC.</returns>
    public double GetTotalUtcOffset(DateTimeOffset utcDate)
    {
        var offset = UtcOffsetHours;
        if (IsDaylightSavingTimeActive(utcDate))
        {
            offset += (DaylightSavingTime?.OffsetHours ?? 0);
        }
        return offset;
    }

    /// <summary>
    /// Converts a UTC time to local time using the timezone information.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert.</param>
    /// <returns>The equivalent local time.</returns>
    public DateTimeOffset ConvertUtcToLocal(DateTimeOffset utcTime)
    {
        return utcTime.AddHours(GetTotalUtcOffset(utcTime));
    }

    /// <summary>
    /// Converts a local time to UTC using the timezone information.
    /// </summary>
    /// <param name="localTime">The local time to convert.</param>
    /// <returns>The equivalent UTC time.</returns>
    public DateTime ConvertLocalToUtc(DateTime localTime)
    {
        // This is an approximation - for exact conversion, we'd need to handle DST transitions more carefully
        var estimatedUtc = localTime.AddHours(-UtcOffsetHours);
        var actualOffset = GetTotalUtcOffset(estimatedUtc);
        return localTime.AddHours(-actualOffset);
    }
}

/// <summary>
/// Represents daylight saving time information including start and end dates.
/// </summary>
public class DaylightSavingTimeInfo
{
    /// <summary>
    /// Gets or sets the month when DST starts (1-12).
    /// </summary>
    public int StartMonth { get; set; } = 3; // March

    /// <summary>
    /// Gets or sets the day of the month when DST starts.
    /// Use 0 for last occurrence of the day, or negative values to count from the end.
    /// </summary>
    public int StartDay { get; set; } = 0; // Last occurrence

    /// <summary>
    /// Gets or sets the day of the week when DST starts.
    /// </summary>
    public DayOfWeek StartDayOfWeek { get; set; } = DayOfWeek.Sunday;

    /// <summary>
    /// Gets or sets the hour when DST starts (0-23).
    /// </summary>
    public int StartHour { get; set; } = 2; // 2:00 AM

    /// <summary>
    /// Gets or sets the month when DST ends (1-12).
    /// </summary>
    public int EndMonth { get; set; } = 11; // November

    /// <summary>
    /// Gets or sets the day of the month when DST ends.
    /// Use 0 for last occurrence of the day, or negative values to count from the end.
    /// </summary>
    public int EndDay { get; set; } = 0; // Last occurrence

    /// <summary>
    /// Gets or sets the day of the week when DST ends.
    /// </summary>
    public DayOfWeek EndDayOfWeek { get; set; } = DayOfWeek.Sunday;

    /// <summary>
    /// Gets or sets the hour when DST ends (0-23).
    /// </summary>
    public int EndHour { get; set; } = 2; // 2:00 AM

    /// <summary>
    /// Gets or sets the DST offset in hours (typically 1.0).
    /// </summary>
    public double OffsetHours { get; set; } = 1.0;

    /// <summary>
    /// Calculates the DST start date for a given year.
    /// </summary>
    /// <param name="year">The year to calculate for.</param>
    /// <returns>The DST start date and time.</returns>
    public DateTime GetStartDate(int year)
    {
        return GetDstDate(year, StartMonth, StartDay, StartDayOfWeek, StartHour);
    }

    /// <summary>
    /// Calculates the DST end date for a given year.
    /// </summary>
    /// <param name="year">The year to calculate for.</param>
    /// <returns>The DST end date and time.</returns>
    public DateTime GetEndDate(int year)
    {
        return GetDstDate(year, EndMonth, EndDay, EndDayOfWeek, EndHour);
    }

    private DateTime GetDstDate(int year, int month, int day, DayOfWeek dayOfWeek, int hour)
    {
        if (day > 0)
        {
            // Specific day of month
            var date = new DateTime(year, month, day, hour, 0, 0);
            return GetNearestDayOfWeek(date, dayOfWeek);
        }
        else
        {
            // Last occurrence or count from end
            var lastDayOfMonth = DateTime.DaysInMonth(year, month);
            var lastDate = new DateTime(year, month, lastDayOfMonth, hour, 0, 0);

            if (day == 0)
            {
                // Last occurrence
                return GetLastDayOfWeek(lastDate, dayOfWeek);
            }
            else
            {
                // Count from end (day is negative)
                var targetDate = lastDate.AddDays(day * 7); // day is negative, so this subtracts weeks
                return GetNearestDayOfWeek(targetDate, dayOfWeek);
            }
        }
    }

    private DateTime GetNearestDayOfWeek(DateTime date, DayOfWeek targetDayOfWeek)
    {
        var daysOffset = (int)targetDayOfWeek - (int)date.DayOfWeek;
        if (daysOffset < 0) daysOffset += 7;
        return date.AddDays(daysOffset);
    }

    private DateTime GetLastDayOfWeek(DateTime date, DayOfWeek targetDayOfWeek)
    {
        var daysOffset = (int)date.DayOfWeek - (int)targetDayOfWeek;
        if (daysOffset < 0) daysOffset += 7;
        return date.AddDays(-daysOffset);
    }
}