using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides methods for serializing and deserializing schedule objects to and from JSON.
/// </summary>
public static class ScheduleSerializer
{
    /// <summary>
    /// Serializes a master schedule to a JSON string.
    /// </summary>
    /// <param name="collection">The master schedule to serialize.</param>
    /// <returns>A JSON string representation of the master schedule.</returns>
    public static string? SerializeScheduleCollection(ScheduleCollection collection)
    {
        var serializable = new SerializableMasterSchedule
        {
            schedules = collection.Schedules?.Select(ConvertToSerializable).ToArray() ?? Array.Empty<SerializableSchedule>(),
            timezone = ConvertTimezoneToSerializable(collection.Timezone)
        };

        var options = new SerializerOptions
        {
            OmitNulls = true,
            WriteIndented = true
        };

        return MicroJson.Serialize(serializable, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a master schedule object.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A master schedule object.</returns>
    /// <exception cref="ArgumentException">Thrown when the JSON contains invalid event types or data.</exception>
    public static ScheduleCollection DeserializeScheduleCollection(string json)
    {
        var serializable = MicroJson.Deserialize<SerializableMasterSchedule>(json);

        if (serializable?.schedules == null)
        {
            throw new ArgumentException("JSON contains no schedule information");
        }

        var collection = new ScheduleCollection(
            serializable.schedules.Select(ConvertFromSerializable)
            );

        if (serializable.timezone != null)
        {
            collection.Timezone = ConvertTimezoneFromSerializable(serializable.timezone);
        }

        return collection;
    }

    /// <summary>
    /// Converts a Schedule object to its serializable representation.
    /// </summary>
    /// <param name="schedule">The schedule to convert.</param>
    /// <returns>A serializable schedule object.</returns>
    private static SerializableSchedule ConvertToSerializable(Schedule schedule)
    {
        return new SerializableSchedule
        {
            name = schedule.Name,
            events = schedule.Events.Select(ConvertEventToSerializable).ToArray()
        };
    }

    /// <summary>
    /// Converts a serializable schedule object back to a Schedule object.
    /// </summary>
    /// <param name="serializable">The serializable schedule to convert.</param>
    /// <returns>A Schedule object.</returns>
    private static Schedule ConvertFromSerializable(SerializableSchedule serializable)
    {
        return new Schedule
        {
            Name = serializable.name,
            Events = serializable.events?.Select(ConvertEventFromSerializable).ToList() ?? new List<IScheduleEvent>()
        };
    }

    /// <summary>
    /// Converts a schedule event to its serializable representation.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to convert.</param>
    /// <returns>A serializable schedule event object.</returns>
    /// <exception cref="ArgumentException">Thrown when the schedule event type is unknown.</exception>
    private static SerializableScheduleEvent ConvertEventToSerializable(IScheduleEvent scheduleEvent)
    {
        var result = new SerializableScheduleEvent
        {
            eventType = scheduleEvent.EventType.ToString(),
            isDisabled = scheduleEvent.IsDisabled
        };

        switch (scheduleEvent)
        {
            case DailyScheduleEvent daily:
                result.data = daily.Data;
                result.eventTime = daily.EventTime.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                break;

            case WeekdayScheduleEvent weekday:
                result.data = weekday.Data;
                result.eventTime = weekday.EventTime.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                result.daysOfWeek = weekday.DaysOfWeek?.Select(d => d.ToString()).ToArray();
                break;

            case SunriseOffsetScheduleEvent sunrise:
                result.data = sunrise.Data;
                result.offset = FormatTimeSpanOffset(sunrise.Offset);
                result.daysOfWeek = sunrise.DaysOfWeek?.Select(d => d.ToString()).ToArray();
                break;

            case SunsetOffsetScheduleEvent sunset:
                result.data = sunset.Data;
                result.offset = FormatTimeSpanOffset(sunset.Offset);
                result.daysOfWeek = sunset.DaysOfWeek?.Select(d => d.ToString()).ToArray();
                break;

            default:
                throw new ArgumentException($"Unknown schedule event type: {scheduleEvent.GetType()}");
        }

        return result;
    }

    /// <summary>
    /// Converts a serializable schedule event back to a schedule event object.
    /// </summary>
    /// <param name="serializable">The serializable schedule event to convert.</param>
    /// <returns>A schedule event object.</returns>
    /// <exception cref="ArgumentException">Thrown when the event type is invalid or unsupported.</exception>
    private static IScheduleEvent ConvertEventFromSerializable(SerializableScheduleEvent serializable)
    {
        if (!Enum.TryParse<ScheduleEventType>(serializable.eventType, out var eventType))
        {
            throw new ArgumentException($"Invalid event type: {serializable.eventType}");
        }

        IScheduleEvent result = eventType switch
        {
            ScheduleEventType.Daily => new DailyScheduleEvent(
                DateTimeOffset.Parse(serializable.eventTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                serializable.data),

            ScheduleEventType.Weekday => new WeekdayScheduleEvent(
                DateTimeOffset.Parse(serializable.eventTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                serializable.data,
                ParseDaysOfWeek(serializable.daysOfWeek) ?? []),

            ScheduleEventType.SunriseOffset => new SunriseOffsetScheduleEvent(
                ParseTimeSpanOffset(serializable.offset),
                serializable.data,
                ParseDaysOfWeek(serializable.daysOfWeek)),

            ScheduleEventType.SunsetOffset => new SunsetOffsetScheduleEvent(
                ParseTimeSpanOffset(serializable.offset),
                serializable.data,
                ParseDaysOfWeek(serializable.daysOfWeek)),

            _ => throw new ArgumentException($"Unsupported event type: {eventType}")
        };

        result.IsDisabled = serializable.isDisabled;
        return result;
    }

    /// <summary>
    /// Parses an array of day-of-week strings into a DayOfWeek array.
    /// </summary>
    /// <param name="daysOfWeekStrings">The array of day-of-week strings to parse.</param>
    /// <returns>An array of DayOfWeek values, or null if the input is null.</returns>
    private static DayOfWeek[]? ParseDaysOfWeek(string[]? daysOfWeekStrings)
    {
        if (daysOfWeekStrings == null)
        {
            return null;
        }

        return daysOfWeekStrings
            .Select(d => Enum.Parse<DayOfWeek>(d))
            .ToArray();
    }

    /// <summary>
    /// Formats a TimeSpan offset into a string representation suitable for JSON serialization.
    /// </summary>
    /// <param name="offset">The TimeSpan offset to format.</param>
    /// <returns>A formatted string representation of the offset.</returns>
    private static string FormatTimeSpanOffset(TimeSpan offset)
    {
        // TimeSpan.ToString with custom format to match JSON format
        if (offset < TimeSpan.Zero)
        {
            return $"-{offset.Negate():hh\\:mm\\:ss}";
        }
        else
        {
            return offset.ToString(@"hh\:mm\:ss");
        }
    }

    /// <summary>
    /// Parses a string representation of a TimeSpan offset.
    /// </summary>
    /// <param name="offsetString">The offset string to parse.</param>
    /// <returns>A TimeSpan representing the offset.</returns>
    /// <exception cref="ArgumentException">Thrown when the offset format is invalid.</exception>
    private static TimeSpan ParseTimeSpanOffset(string? offsetString)
    {
        if (string.IsNullOrEmpty(offsetString))
        {
            return TimeSpan.Zero;
        }

        // Handle negative offsets
        var isNegative = offsetString.StartsWith("-");
        var timeString = isNegative ? offsetString.Substring(1) : offsetString;

        if (TimeSpan.TryParse(timeString, out var timeSpan))
        {
            return isNegative ? timeSpan.Negate() : timeSpan;
        }

        throw new ArgumentException($"Invalid offset format: {offsetString}");
    }

    /// <summary>
    /// Converts a TimezoneInfo object to its serializable representation.
    /// </summary>
    /// <param name="timezone">The timezone info to convert.</param>
    /// <returns>A serializable timezone object.</returns>
    private static SerializableTimezone? ConvertTimezoneToSerializable(TimezoneInfo? timezone)
    {
        if (timezone == null) return null;

        return new SerializableTimezone
        {
            timezoneName = timezone.TimezoneName,
            utcOffsetHours = timezone.UtcOffsetHours,
            daylightSavingTime = timezone.DaylightSavingTime != null ? new SerializableDaylightSavingTime
            {
                startMonth = timezone.DaylightSavingTime.StartMonth,
                startDay = timezone.DaylightSavingTime.StartDay,
                startDayOfWeek = timezone.DaylightSavingTime.StartDayOfWeek.ToString(),
                startHour = timezone.DaylightSavingTime.StartHour,
                endMonth = timezone.DaylightSavingTime.EndMonth,
                endDay = timezone.DaylightSavingTime.EndDay,
                endDayOfWeek = timezone.DaylightSavingTime.EndDayOfWeek.ToString(),
                endHour = timezone.DaylightSavingTime.EndHour,
                offsetHours = timezone.DaylightSavingTime.OffsetHours
            } : null
        };
    }

    /// <summary>
    /// Converts a serializable timezone object back to a TimezoneInfo object.
    /// </summary>
    /// <param name="serializable">The serializable timezone to convert.</param>
    /// <returns>A TimezoneInfo object.</returns>
    private static TimezoneInfo ConvertTimezoneFromSerializable(SerializableTimezone serializable)
    {
        var timezone = new TimezoneInfo
        {
            TimezoneName = serializable.timezoneName ?? "UTC",
            UtcOffsetHours = serializable.utcOffsetHours
        };

        if (serializable.daylightSavingTime != null)
        {
            timezone.DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = serializable.daylightSavingTime.startMonth,
                StartDay = serializable.daylightSavingTime.startDay,
                StartDayOfWeek = Enum.Parse<DayOfWeek>(serializable.daylightSavingTime.startDayOfWeek),
                StartHour = serializable.daylightSavingTime.startHour,
                EndMonth = serializable.daylightSavingTime.endMonth,
                EndDay = serializable.daylightSavingTime.endDay,
                EndDayOfWeek = Enum.Parse<DayOfWeek>(serializable.daylightSavingTime.endDayOfWeek),
                EndHour = serializable.daylightSavingTime.endHour,
                OffsetHours = serializable.daylightSavingTime.offsetHours
            };
        }

        return timezone;
    }
}

/// <summary>
/// Internal serializable representation of a master schedule for JSON conversion.
/// </summary>
internal class SerializableMasterSchedule
{
    /// <summary>
    /// Gets or sets the array of serializable schedules.
    /// </summary>
    public SerializableSchedule[] schedules { get; set; } = Array.Empty<SerializableSchedule>();

    /// <summary>
    /// Gets or sets the timezone information.
    /// </summary>
    public SerializableTimezone? timezone { get; set; }
}

/// <summary>
/// Internal serializable representation of a schedule for JSON conversion.
/// </summary>
internal class SerializableSchedule
{
    /// <summary>
    /// Gets or sets the schedule name.
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the array of serializable schedule events.
    /// </summary>
    public SerializableScheduleEvent[] events { get; set; } = Array.Empty<SerializableScheduleEvent>();
}

/// <summary>
/// Internal serializable representation of a schedule event for JSON conversion.
/// </summary>
internal class SerializableScheduleEvent
{
    /// <summary>
    /// Gets or sets the event type as a string.
    /// </summary>
    public string eventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the event is disabled.
    /// </summary>
    public bool isDisabled { get; set; }

    /// <summary>
    /// Gets or sets the data for the event.
    /// </summary>
    public string? data { get; set; }

    /// <summary>
    /// Gets or sets the event time for Daily and Weekday events.
    /// </summary>
    public string? eventTime { get; set; }

    /// <summary>
    /// Gets or sets the offset for Sunrise/Sunset offset events in TimeSpan format.
    /// </summary>
    public string? offset { get; set; }

    /// <summary>
    /// Gets or sets the days of week for Weekday and offset events.
    /// </summary>
    public string[]? daysOfWeek { get; set; }
}

/// <summary>
/// Internal serializable representation of timezone information for JSON conversion.
/// </summary>
internal class SerializableTimezone
{
    /// <summary>
    /// Gets or sets the timezone name.
    /// </summary>
    public string timezoneName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC offset in hours.
    /// </summary>
    public double utcOffsetHours { get; set; }

    /// <summary>
    /// Gets or sets the daylight saving time information.
    /// </summary>
    public SerializableDaylightSavingTime? daylightSavingTime { get; set; }
}

/// <summary>
/// Internal serializable representation of daylight saving time information for JSON conversion.
/// </summary>
internal class SerializableDaylightSavingTime
{
    /// <summary>
    /// Gets or sets the start month.
    /// </summary>
    public int startMonth { get; set; }

    /// <summary>
    /// Gets or sets the start day.
    /// </summary>
    public int startDay { get; set; }

    /// <summary>
    /// Gets or sets the start day of week.
    /// </summary>
    public string startDayOfWeek { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start hour.
    /// </summary>
    public int startHour { get; set; }

    /// <summary>
    /// Gets or sets the end month.
    /// </summary>
    public int endMonth { get; set; }

    /// <summary>
    /// Gets or sets the end day.
    /// </summary>
    public int endDay { get; set; }

    /// <summary>
    /// Gets or sets the end day of week.
    /// </summary>
    public string endDayOfWeek { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the end hour.
    /// </summary>
    public int endHour { get; set; }

    /// <summary>
    /// Gets or sets the DST offset in hours.
    /// </summary>
    public double offsetHours { get; set; }
}