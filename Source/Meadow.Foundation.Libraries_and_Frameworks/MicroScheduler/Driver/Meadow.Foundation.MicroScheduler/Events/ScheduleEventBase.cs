namespace Meadow.Foundation.Scheduling;

public abstract class ScheduleEventBase : IScheduleEvent
{
    /// <summary>
    /// Gets the type of this schedule event.
    /// </summary>
    public abstract ScheduleEventType EventType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this event is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Gets or sets data that is passed along with this event from the defined schedule
    /// </summary>
    public string? Data { get; set; }
}
