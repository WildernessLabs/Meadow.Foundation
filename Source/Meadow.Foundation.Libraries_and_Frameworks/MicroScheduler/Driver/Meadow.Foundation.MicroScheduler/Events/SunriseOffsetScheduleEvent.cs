using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule event that triggers at a specific time offset from sunrise.
/// </summary>
public class SunriseOffsetScheduleEvent : ScheduleEventBase
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public override ScheduleEventType EventType => ScheduleEventType.SunriseOffset;

    /// <summary>
    /// Gets the time offset from sunrise when this event should trigger.
    /// Positive values represent time after sunrise, negative values represent time before sunrise.
    /// </summary>
    public TimeSpan Offset { get; }

    /// <summary>
    /// Gets the days of the week when this event should be active.
    /// If null, the event is active every day.
    /// </summary>
    public DayOfWeek[]? DaysOfWeek { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SunriseOffsetScheduleEvent"/> class.
    /// </summary>
    /// <param name="offset">The time offset from sunrise when this event should trigger.</param>
    /// <param name="data">Data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    public SunriseOffsetScheduleEvent(TimeSpan offset, string? data = null, DayOfWeek[]? daysOfWeek = null)
    {
        Offset = offset;
        Data = data;
        DaysOfWeek = daysOfWeek;
    }
}