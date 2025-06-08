using System;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides data for the ScheduleEventTriggered event.
/// </summary>
public class ScheduleEventTriggeredEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the circuit that was affected by the schedule event.
    /// </summary>
    public string CircuitName { get; }

    /// <summary>
    /// Gets the schedule event that was triggered.
    /// </summary>
    public IScheduleEvent ScheduleEvent { get; }

    /// <summary>
    /// Gets the data for this event
    /// </summary>
    public string? Data { get; }

    /// <summary>
    /// Gets the date and time when the event was triggered.
    /// </summary>
    public DateTimeOffset TriggeredAt { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleEventTriggeredEventArgs"/> class.
    /// </summary>
    /// <param name="circuitName">The name of the circuit that was affected.</param>
    /// <param name="scheduleEvent">The schedule event that was triggered.</param>
    /// <param name="newState">The new state that the circuit was set to.</param>
    /// <param name="data">Data associated with this event.</param>
    public ScheduleEventTriggeredEventArgs(string circuitName, IScheduleEvent scheduleEvent, string? data, DateTimeOffset triggeredAt)
    {
        CircuitName = circuitName;
        ScheduleEvent = scheduleEvent;
        Data = data;
        TriggeredAt = triggeredAt;
    }
}