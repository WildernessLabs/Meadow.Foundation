using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
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
            schedules = collection.Schedules?.Select(ConvertToSerializable).ToArray()
        };

        return MicroJson.Serialize(serializable);
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

        return new ScheduleCollection(
            serializable.schedules.Select(ConvertFromSerializable)
            );
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
            circuitName = schedule.Name,
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
            Name = serializable.circuitName,
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
                result.eventTime = daily.EventTime.ToString("yyyy-MM-ddTHH:mm:ss");
                break;

            case WeekdayScheduleEvent weekday:
                result.data = weekday.Data;
                result.eventTime = weekday.EventTime.ToString("yyyy-MM-ddTHH:mm:ss");
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
                DateTime.Parse(serializable.eventTime),
                serializable.data),

            ScheduleEventType.Weekday => new WeekdayScheduleEvent(
                DateTime.Parse(serializable.eventTime),
                serializable.data,
                ParseDaysOfWeek(serializable.daysOfWeek)),

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
    private static DayOfWeek[] ParseDaysOfWeek(string[] daysOfWeekStrings)
    {
        if (daysOfWeekStrings == null)
            return null;

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
    private static TimeSpan ParseTimeSpanOffset(string offsetString)
    {
        if (string.IsNullOrEmpty(offsetString))
            return TimeSpan.Zero;

        // Handle negative offsets
        var isNegative = offsetString.StartsWith("-");
        var timeString = isNegative ? offsetString.Substring(1) : offsetString;

        if (TimeSpan.TryParse(timeString, out var timeSpan))
        {
            return isNegative ? timeSpan.Negate() : timeSpan;
        }

        throw new ArgumentException($"Invalid offset format: {offsetString}");
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
    public SerializableSchedule[] schedules { get; set; }
}

/// <summary>
/// Internal serializable representation of a schedule for JSON conversion.
/// </summary>
internal class SerializableSchedule
{
    /// <summary>
    /// Gets or sets the circuit name.
    /// </summary>
    public string circuitName { get; set; }

    /// <summary>
    /// Gets or sets the array of serializable schedule events.
    /// </summary>
    public SerializableScheduleEvent[] events { get; set; }
}

/// <summary>
/// Internal serializable representation of a schedule event for JSON conversion.
/// </summary>
internal class SerializableScheduleEvent
{
    /// <summary>
    /// Gets or sets the event type as a string.
    /// </summary>
    public string eventType { get; set; }

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
    public string eventTime { get; set; }

    /// <summary>
    /// Gets or sets the offset for Sunrise/Sunset offset events in TimeSpan format.
    /// </summary>
    public string offset { get; set; }

    /// <summary>
    /// Gets or sets the days of week for Weekday and offset events.
    /// </summary>
    public string[] daysOfWeek { get; set; }
}