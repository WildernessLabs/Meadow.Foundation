using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Represents a schedule for a specific circuit, containing multiple schedule events.
/// </summary>
public class Schedule
{
    /// <summary>
    /// Occurs when a schedule event is triggered and a circuit state is changed.
    /// </summary>
    public event EventHandler<ScheduleEventTriggeredEventArgs>? ScheduleEventTriggered;

    /// <summary>
    /// Gets or sets the name of the circuit this schedule controls.
    /// </summary>
    public string Name { get; set; } = "Schedule";

    /// <summary>
    /// Gets or sets the list of schedule events for this circuit.
    /// </summary>
    public List<IScheduleEvent> Events { get; set; } = new();

    internal void RaiseScheduleEvent(ScheduleEventTriggeredEventArgs e)
    {
        ScheduleEventTriggered?.Invoke(this, e);
    }

    /// <summary>
    /// Gets a value indicating whether this schedule contains any sunrise or sunset offset events.
    /// This property is used to determine if sunrise/sunset calculations are needed for this specific schedule.
    /// </summary>
    [JsonIgnore]
    public bool ContainsSunriseOrSunsetEvents
    {
        get => Events.Any(e => e.EventType is ScheduleEventType.SunriseOffset || e.EventType is ScheduleEventType.SunsetOffset);
    }

    /// <summary>
    /// Gets the most recent event that has fired before or at the specified date/time.
    /// This determines the current state of the schedule at the given time.
    /// </summary>
    /// <param name="currentTime">The date/time to check for the most recent event.</param>
    /// <param name="sunTimes">Optional sunrise and sunset times for the current day, required if the schedule contains sunrise/sunset offset events.</param>
    /// <returns>The most recent event that has fired, or null if no events have fired yet.</returns>
    public IScheduleEvent? GetActiveEvent(DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes = null)
    {
        if (Events == null || !Events.Any())
        {
            return null;
        }

        if (ContainsSunriseOrSunsetEvents && sunTimes == null)
        {
            throw new ArgumentException("Sunrise/sunset times are required when the schedule contains sunrise or sunset offset events.", nameof(sunTimes));
        }

        var mostRecentEvent = (Event: (IScheduleEvent?)null, TriggerTime: DateTimeOffset.MinValue);
        var currentDate = new DateTimeOffset(currentTime.Date, TimeSpan.Zero);
        var previousDate = currentDate.AddDays(-1);

        // Check events from current day and previous day to find most recent
        var datesToCheck = new[] { previousDate, currentDate };

        foreach (var dateToCheck in datesToCheck)
        {
            foreach (var scheduleEvent in Events)
            {
                if (scheduleEvent.IsDisabled)
                    continue;

                var triggerTime = GetEventTriggerTimeForDate(scheduleEvent, dateToCheck, sunTimes);

                // Only consider events that have already occurred (triggerTime <= currentTime)
                if (triggerTime.HasValue && triggerTime <= currentTime && triggerTime > mostRecentEvent.TriggerTime)
                {
                    mostRecentEvent = (scheduleEvent, triggerTime.Value);
                }
            }
        }

        return mostRecentEvent.Event;
    }

    /// <summary>
    /// Gets the next event that will occur after the specified date/time.
    /// </summary>
    /// <param name="currentTime">The date/time to find the next event after.</param>
    /// <param name="sunTimes">Optional sunrise and sunset times for the current day, required if the schedule contains sunrise/sunset offset events.</param>
    /// <param name="lookAheadDays">Number of days to look ahead when searching for the next event (default: 7).</param>
    /// <returns>A tuple containing the next event and its calculated trigger time, or null if no future events are found within the look-ahead period.</returns>
    public (IScheduleEvent Event, DateTimeOffset TriggerTime)? GetNextEvent(DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes = null, int lookAheadDays = 7)
    {
        if (Events == null || !Events.Any())
        {
            return null;
        }

        if (ContainsSunriseOrSunsetEvents && sunTimes == null)
        {
            throw new ArgumentException("Sunrise/sunset times are required when the schedule contains sunrise or sunset offset events.", nameof(sunTimes));
        }

        var nextEvents = new List<(IScheduleEvent Event, DateTimeOffset TriggerTime)>();
        var searchEndTime = currentTime.AddDays(lookAheadDays);

        foreach (var scheduleEvent in Events)
        {
            if (scheduleEvent.IsDisabled)
                continue;

            var nextTriggerTime = GetNextTriggerTime(scheduleEvent, currentTime, sunTimes, searchEndTime);
            if (nextTriggerTime.HasValue)
            {
                nextEvents.Add((scheduleEvent, nextTriggerTime.Value));
            }
        }

        if (!nextEvents.Any())
        {
            return null;
        }

        // Return the earliest next event
        return nextEvents
            .OrderBy(e => e.TriggerTime)
            .First();
    }

    private DateTimeOffset? GetNextTriggerTime(IScheduleEvent scheduleEvent, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes, DateTimeOffset searchEndTime)
    {
        var checkDate = new DateTimeOffset(currentTime.Date, TimeSpan.Zero);

        while (checkDate.Date <= searchEndTime.Date)
        {
            var triggerTime = GetEventTriggerTime(scheduleEvent, checkDate, sunTimes);

            if (triggerTime.HasValue && triggerTime > currentTime)
            {
                return triggerTime;
            }

            checkDate = checkDate.AddDays(1);
        }

        return null;
    }

    private DateTimeOffset? GetEventTriggerTime(IScheduleEvent scheduleEvent, DateTimeOffset date, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        var dayOfWeek = date.DayOfWeek;

        return scheduleEvent switch
        {
            DailyScheduleEvent daily =>
                new DateTimeOffset(date.Date.Add(daily.EventTime.TimeOfDay), TimeSpan.Zero),

            WeekdayScheduleEvent weekday =>
                weekday.DaysOfWeek.Contains(dayOfWeek)
                    ? new DateTimeOffset(date.Date.Add(weekday.EventTime.TimeOfDay), TimeSpan.Zero)
                    : null,

            SunriseOffsetScheduleEvent sunrise =>
                (sunrise.DaysOfWeek == null || sunrise.DaysOfWeek.Contains(dayOfWeek))
                    ? sunTimes!.Value.Sunrise.Date == date.Date
                        ? sunTimes.Value.Sunrise.Add(sunrise.Offset)
                        : null
                    : null,

            SunsetOffsetScheduleEvent sunset =>
                (sunset.DaysOfWeek == null || sunset.DaysOfWeek.Contains(dayOfWeek))
                    ? sunTimes!.Value.Sunset.Date == date.Date
                        ? sunTimes.Value.Sunset.Add(sunset.Offset)
                        : null
                    : null,

            _ => null
        };
    }

    private DateTimeOffset? GetEventTriggerTimeForDate(IScheduleEvent scheduleEvent, DateTimeOffset date, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        var dayOfWeek = date.DayOfWeek;

        return scheduleEvent switch
        {
            DailyScheduleEvent daily =>
                new DateTimeOffset(date.Date.Add(daily.EventTime.TimeOfDay), TimeSpan.Zero),

            WeekdayScheduleEvent weekday =>
                weekday.DaysOfWeek.Contains(dayOfWeek)
                    ? new DateTimeOffset(date.Date.Add(weekday.EventTime.TimeOfDay), TimeSpan.Zero)
                    : null,

            SunriseOffsetScheduleEvent sunrise =>
                (sunrise.DaysOfWeek == null || sunrise.DaysOfWeek.Contains(dayOfWeek))
                    ? new DateTimeOffset(date.Date, TimeSpan.Zero).Add(sunTimes!.Value.Sunrise.TimeOfDay).Add(sunrise.Offset)
                    : null,

            SunsetOffsetScheduleEvent sunset =>
                (sunset.DaysOfWeek == null || sunset.DaysOfWeek.Contains(dayOfWeek))
                    ? new DateTimeOffset(date.Date, TimeSpan.Zero).Add(sunTimes!.Value.Sunset.TimeOfDay).Add(sunset.Offset)
                    : null,

            _ => null
        };
    }

    private bool IsTimeMatch(TimeSpan eventTime, TimeSpan currentTime)
    {
        // Match if we're within the same minute
        var eventMinutes = (int)eventTime.TotalMinutes;
        var currentMinutes = (int)currentTime.TotalMinutes;
        return eventMinutes == currentMinutes;
    }

    private int GetEventPriority(IScheduleEvent scheduleEvent)
    {
        // Lower numbers = higher priority
        return scheduleEvent switch
        {
            WeekdayScheduleEvent => 1,     // Highest priority - most specific
            SunriseOffsetScheduleEvent => 2, // Medium-high priority
            SunsetOffsetScheduleEvent => 2,  // Medium-high priority  
            DailyScheduleEvent => 3,       // Lowest priority - most general
            _ => 999
        };
    }

    private int GetEventSpecificity(IScheduleEvent scheduleEvent)
    {
        // Used as tie-breaker for same priority events
        // Lower numbers = more specific
        return scheduleEvent switch
        {
            WeekdayScheduleEvent weekday => weekday.DaysOfWeek?.Length ?? 7,
            SunriseOffsetScheduleEvent sunrise => sunrise.DaysOfWeek?.Length ?? 7,
            SunsetOffsetScheduleEvent sunset => sunset.DaysOfWeek?.Length ?? 7,
            DailyScheduleEvent => 7,
            _ => 999
        };
    }
}