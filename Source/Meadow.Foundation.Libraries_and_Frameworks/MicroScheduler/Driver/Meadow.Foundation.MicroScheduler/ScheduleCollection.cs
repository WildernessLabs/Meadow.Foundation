using Meadow.Foundation.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a collection of schedules that can be managed as a single unit.
/// </summary>
public class ScheduleCollection : IEnumerable<Schedule>
{
    internal SemaphoreSlim SyncRoot { get; } = new SemaphoreSlim(1, 1);

    private readonly List<Schedule> _schedules = new();

    /// <summary>
    /// Gets or sets the timezone information for this schedule collection.
    /// </summary>
    public TimezoneInfo Timezone { get; set; } = new();

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
        _schedules = schedules.ToList();
    }

    /// <summary>
    /// Gets the number of schedules in the collection.
    /// </summary>
    public int Count => _schedules.Count;

    /// <summary>
    /// Gets a schedule by its zero-based index.
    /// </summary>
    /// <param name="index">The zero-based index of the schedule to get.</param>
    /// <returns>The schedule at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
    public Schedule this[int index] => _schedules[index];

    /// <summary>
    /// Gets a schedule by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name of the schedule to get.</param>
    /// <returns>The schedule with the specified name, or null if not found.</returns>
    public Schedule? this[string name] => _schedules.FirstOrDefault(s =>
        string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));


    /// <summary>
    /// Adds a schedule to the collection.
    /// </summary>
    /// <param name="schedule">The schedule to add.</param>
    public void Add(Schedule schedule)
    {
        _schedules.Add(schedule);
    }

    /// <summary>
    /// Removes the specified schedule from the collection.
    /// </summary>
    /// <remarks>If the specified schedule is not found in the collection, no action is taken.</remarks>
    /// <param name="schedule">The schedule to remove from the collection. Cannot be <see langword="null"/>.</param>
    public void Remove(Schedule schedule)
    {
        _schedules.Remove(schedule);
    }

    /// <summary>
    /// Gets the internal schedules list for serialization purposes.
    /// </summary>
    internal List<Schedule> Schedules => _schedules;

    /// <summary>
    /// Gets a value indicating whether any of the schedules contain sunrise or sunset events.
    /// This property is used to determine if sunrise/sunset calculations are needed.
    /// </summary>
    [JsonIgnore]
    public bool ContainsSunriseOrSunsetEvents => _schedules.Any(s => s.ContainsSunriseOrSunsetEvents);

    /// <inheritdoc/>
    public IEnumerator<Schedule> GetEnumerator()
    {
        return _schedules.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}