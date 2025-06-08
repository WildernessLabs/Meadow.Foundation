using Meadow.Foundation.Scheduling;
using Meadow.Foundation.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

/// <summary>
/// Represents a collection of schedules that can be managed as a single unit.
/// </summary>
public class ScheduleCollection : IEnumerable<Schedule>
{
    internal SemaphoreSlim SyncRoot { get; } = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Gets or sets the array of schedules managed by this master schedule.
    /// </summary>
    public List<Schedule> Schedules { get; set; } = new();

    public static ScheduleCollection LoadFrom(FileInfo scheduleFile)
    {
        if (!scheduleFile.Exists) throw new FileNotFoundException();
        var json = File.ReadAllText(scheduleFile.FullName);
        return ScheduleSerializer.DeserializeScheduleCollection(json);
    }

    public ScheduleCollection()
    {
    }

    public ScheduleCollection(IEnumerable<Schedule> schedules)
    {
        Schedules = schedules.ToList();
    }

    /// <summary>
    /// Gets a value indicating whether any of the schedules contain sunrise or sunset events.
    /// This property is used to determine if sunrise/sunset calculations are needed.
    /// </summary>
    [JsonIgnore]
    public bool ContainsSunriseOrSunsetEvents => Schedules.Any(s => s.ContainsSunriseOrSunsetEvents);

    /// <inheritdoc/>
    public IEnumerator<Schedule> GetEnumerator()
    {
        return Schedules.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}