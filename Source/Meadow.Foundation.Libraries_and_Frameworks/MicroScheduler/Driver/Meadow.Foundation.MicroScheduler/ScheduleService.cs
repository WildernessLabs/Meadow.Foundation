using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides a service for managing and executing scheduled events.
/// The service evaluates schedules periodically and triggers state change events as needed.
/// </summary>
public class ScheduleService : IDisposable
{
    private readonly ITimeProvider _timeProvider;
    private Timer? _timer;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    private ScheduleCollection? _scheduleCollection = null;

    /// <summary>
    /// Occurs when a schedule event is triggered and a circuit state is changed.
    /// </summary>
    public event EventHandler<ScheduleEventTriggeredEventArgs>? ScheduleEventTriggered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleService"/> class using the system time provider.
    /// </summary>
    public ScheduleService()
        : this(new SystemTimeProvider())
    {
    }

    /// <summary>
    /// Gets the current time adjusted for any schedule timezone and DST offsets
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset GetAdjustedTime()
    {
        var utc = _timeProvider.GetUtcNow().GetAwaiter().GetResult();

        return GetAdjustedTime(utc);
    }

    /// <summary>
    /// Gets the current time adjusted for any schedule timezone and DST offsets
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset GetAdjustedTime(DateTimeOffset utc)
    {
        if (_scheduleCollection != null)
        {
            var totalOffset = _scheduleCollection.Timezone.GetTotalUtcOffset(utc.DateTime);
            utc = utc.AddHours(totalOffset);
        }

        return utc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleService"/> class with custom providers.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for getting current time and sunrise/sunset information.</param>
    public ScheduleService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Returns true if the service has been disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Starts the schedule service timer to begin evaluating schedules.
    /// The timer runs every minute, starting at the next minute boundary.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task Start()
    {
        if (_timer == null)
        {
            // Create timer that ticks every minute, starting at the next minute boundary
            var now = await _timeProvider.GetUtcNow();
            var nextMinute = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeSpan.Zero).AddMinutes(1);
            var initialDelay = nextMinute - now;

            _timer = new Timer(OnTimerTickCallback, null, initialDelay, TimeSpan.FromMinutes(1));
        }
    }

    /// <summary>
    /// Sets the master schedule that this service will evaluate.
    /// </summary>
    /// <param name="masterSchedule">The collection of schedules for the servcie</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    public async Task SetSchedules(ScheduleCollection schedules)
    {
        await _lock.WaitAsync();
        try
        {
            _scheduleCollection = schedules;

        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Manually applies the current schedule immediately, evaluating all schedules against the current time.
    /// </summary>
    /// <returns>A task that represents the asynchronous apply operation.</returns>
    public async Task ApplyScheduleAsync()
    {
        await OnTimerTick();
    }

    /// <summary>
    /// Manually applies the current schedule immediately, evaluating all schedules against the current time.
    /// This is a synchronous version for backwards compatibility.
    /// </summary>
    public void ApplySchedule()
    {
        // Synchronous version for backwards compatibility
        OnTimerTick().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Timer callback method that triggers schedule evaluation.
    /// </summary>
    /// <param name="_">Unused timer parameter.</param>
    private void OnTimerTickCallback(object? _)
    {
        // Fire and forget for timer callbacks
        _ = Task.Run(async () => await OnTimerTick());
    }

    /// <summary>
    /// Performs the main schedule evaluation logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous evaluation operation.</returns>
    private async Task OnTimerTick()
    {
        if (IsDisposed || _scheduleCollection == null)
        {
            return;
        }

        await _scheduleCollection.SyncRoot.WaitAsync();

        try
        {
            await EvaluateSchedules();
        }
        catch (Exception ex)
        {
            // Log error but don't let it kill the timer
            OnScheduleError(ex);
        }
        finally
        {
            _scheduleCollection.SyncRoot.Release();
        }
    }

    /// <summary>
    /// Updates the current location with the specified latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude of the new location, in decimal degrees. Must be in the range -90 to 90.</param>
    /// <param name="longitude">The longitude of the new location, in decimal degrees. Must be in the range -180 to 180.</param>
    public void UpdateLocation(double latitude, double longitude)
    {
        _timeProvider.UpdateLocation(latitude, longitude);
    }

    /// <summary>
    /// Evaluates all schedules against the current time and updates circuit states as needed.
    /// </summary>
    /// <returns>A task that represents the asynchronous evaluation operation.</returns>
    private async Task EvaluateSchedules()
    {
        if (_scheduleCollection?.Count == 0)
        {
            return;
        }

        var currentTime = await _timeProvider.GetUtcNow();
        (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes = null;

        if (_scheduleCollection?.ContainsSunriseOrSunsetEvents ?? false)
        {
            sunTimes = await _timeProvider.GetUtcSunriseAndSunset();
        }

        if (_scheduleCollection != null)
        {
            foreach (var schedule in _scheduleCollection)
            {
                var activeEvents = EvaluateSchedule(schedule, currentTime, sunTimes);

                if (activeEvents != null)
                {
                    foreach (var evt in activeEvents)
                    {
                        RaiseScheduleEvents(new ScheduleEventTriggeredEventArgs(
                            schedule,
                            evt.Event,
                            evt.Event.Data,
                            currentTime));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Evaluates the desired state for a specific circuit based on its schedule and current time.
    /// </summary>
    /// <param name="schedule">The schedule to evaluate.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="sunTimes">The sunrise and sunset times for the current day, if needed.</param>
    /// <returns>True if events are active, otehrwise falseThe desired state for the circuit, or null if no events are active.</returns>
    private IEnumerable<(IScheduleEvent Event, int Priority)>? EvaluateSchedule(Schedule schedule, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        if (schedule.Events == null || !schedule.Events.Any())
        {
            return null;
        }

        // Find all events that should be active right now
        var activeEvents = new List<(IScheduleEvent Event, int Priority)>();

        foreach (var scheduleEvent in schedule.Events)
        {
            if (scheduleEvent.IsDisabled)
                continue;

            if (IsEventActive(scheduleEvent, currentTime, sunTimes))
            {
                var priority = GetEventPriority(scheduleEvent);
                activeEvents.Add((scheduleEvent, priority));
            }
        }

        if (!activeEvents.Any())
        {
            return null;
        }

        // Use the highest priority event (lowest number = highest priority)
        var highestPriorityEvent = activeEvents
            .OrderBy(e => e.Priority)
            .ThenBy(e => GetEventSpecificity(e.Event)) // More specific events win
            .First();

        return activeEvents;
    }

    /// <summary>
    /// Determines if a schedule event is currently active based on the current time.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to check.</param>
    /// <param name="currentTime">The current time.</param>
    /// <param name="sunTimes">The sunrise and sunset times for the current day, if needed.</param>
    /// <returns>True if the event is active; otherwise, false.</returns>
    private bool IsEventActive(IScheduleEvent scheduleEvent, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset)? sunTimes)
    {
        var currentTimeOfDay = currentTime.TimeOfDay;
        var currentDayOfWeek = currentTime.DayOfWeek;

        return scheduleEvent switch
        {
            DailyScheduleEvent daily =>
                IsTimeMatch(daily.EventTime.TimeOfDay, currentTimeOfDay),

            WeekdayScheduleEvent weekday =>
                weekday.DaysOfWeek.Contains(currentDayOfWeek) &&
                IsTimeMatch(weekday.EventTime.TimeOfDay, currentTimeOfDay),

            SunriseOffsetScheduleEvent sunrise =>
                (sunrise.DaysOfWeek == null || sunrise.DaysOfWeek.Contains(currentDayOfWeek)) &&
                IsTimeMatch(sunTimes!.Value.Sunrise.Add(sunrise.Offset).TimeOfDay, currentTimeOfDay),

            SunsetOffsetScheduleEvent sunset =>
                (sunset.DaysOfWeek == null || sunset.DaysOfWeek.Contains(currentDayOfWeek)) &&
                IsTimeMatch(sunTimes!.Value.Sunset.Add(sunset.Offset).TimeOfDay, currentTimeOfDay),

            _ => false
        };
    }

    /// <summary>
    /// Determines if two times match within the same minute.
    /// </summary>
    /// <param name="eventTime">The event time to match.</param>
    /// <param name="currentTime">The current time.</param>
    /// <returns>True if the times are within the same minute; otherwise, false.</returns>
    private bool IsTimeMatch(TimeSpan eventTime, TimeSpan currentTime)
    {
        // Match if we're within the same minute
        var eventMinutes = (int)eventTime.TotalMinutes;
        var currentMinutes = (int)currentTime.TotalMinutes;
        return eventMinutes == currentMinutes;
    }

    /// <summary>
    /// Gets the priority level for a schedule event type.
    /// Lower numbers indicate higher priority.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to get priority for.</param>
    /// <returns>The priority level as an integer.</returns>
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

    /// <summary>
    /// Gets the specificity level for a schedule event, used as a tie-breaker for same priority events.
    /// Lower numbers indicate more specific events.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to get specificity for.</param>
    /// <returns>The specificity level as an integer.</returns>
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

    /// <summary>
    /// Raises the ScheduleEventTriggered event.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    protected virtual void RaiseScheduleEvents(ScheduleEventTriggeredEventArgs e)
    {
        ScheduleEventTriggered?.Invoke(this, e);
        e.Schedule.RaiseScheduleEvent(e);
    }

    /// <summary>
    /// Handles schedule errors. Override this method to implement custom error handling/logging.
    /// </summary>
    /// <param name="exception">The exception that occurred during schedule evaluation.</param>
    protected virtual void OnScheduleError(Exception exception)
    {
        // Override this method to implement custom error handling/logging
        // For now, we'll just swallow the exception to keep the timer running
    }

    /// <summary>
    /// Releases all resources used by the ScheduleService.
    /// </summary>
    public void Dispose()
    {
        if (!IsDisposed)
        {
            _timer?.Dispose();
            _lock?.Dispose();
            IsDisposed = true;
        }
    }
}