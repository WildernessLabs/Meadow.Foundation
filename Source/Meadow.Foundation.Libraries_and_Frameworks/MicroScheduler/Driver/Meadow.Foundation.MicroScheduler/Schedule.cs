using Meadow.Foundation.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule for a specific circuit, containing multiple schedule events.
/// </summary>
public class Schedule
{
    /// <summary>
    /// Gets or sets the name of the circuit this schedule controls.
    /// </summary>
    public string Name { get; set; } = "Schedule";

    /// <summary>
    /// Gets or sets the list of schedule events for this circuit.
    /// </summary>
    public List<IScheduleEvent> Events { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether this schedule contains any sunrise or sunset offset events.
    /// This property is used to determine if sunrise/sunset calculations are needed for this specific schedule.
    /// </summary>
    [JsonIgnore]
    public bool ContainsSunriseOrSunsetEvents
    {
        get => Events.Any(e => e.EventType is ScheduleEventType.SunriseOffset || e.EventType is ScheduleEventType.SunsetOffset);
    }
}