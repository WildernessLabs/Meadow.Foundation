using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule event that triggers on specific days of the week at a specific time.
/// </summary>
public class WeekdayScheduleEvent : ScheduleEventBase
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public override ScheduleEventType EventType => ScheduleEventType.Weekday;

    /// <summary>
    /// Gets the time when this event should trigger (in UTC).
    /// </summary>
    public DateTimeOffset EventTime { get; }

    /// <summary>
    /// Gets the days of the week when this event should be active.
    /// </summary>
    public DayOfWeek[] DaysOfWeek { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeekdayScheduleEvent"/> class.
    /// </summary>
    /// <param name="eventTimeUtc">The time when this event should trigger (in UTC).</param>
    /// <param name="data">Data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active.</param>
    public WeekdayScheduleEvent(DateTimeOffset eventTimeUtc, string? data, DayOfWeek[] daysOfWeek)
    {
        EventTime = eventTimeUtc;
        Data = data;
        DaysOfWeek = daysOfWeek;
    }
}