using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule event that triggers daily at a specific time.
/// </summary>
public class DailyScheduleEvent : ScheduleEventBase
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public override ScheduleEventType EventType => ScheduleEventType.Daily;

    /// <summary>
    /// Gets the time when this event should trigger daily (in UTC).
    /// </summary>
    public DateTimeOffset EventTime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DailyScheduleEvent"/> class.
    /// </summary>
    /// <param name="eventTimeUtc">The time when this event should trigger daily (in UTC).</param>
    /// <param name="data">Data to be passed when this event triggers.</param>
    public DailyScheduleEvent(DateTimeOffset eventTimeUtc, string? data = null)
    {
        EventTime = eventTimeUtc;
        Data = data;
    }
}