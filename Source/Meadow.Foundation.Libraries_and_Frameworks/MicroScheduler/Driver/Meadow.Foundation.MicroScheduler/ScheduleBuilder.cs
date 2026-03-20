using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides a fluent API for building schedule objects with multiple events.
/// </summary>
public class ScheduleBuilder
{
    private readonly string _scheduleName;
    private readonly List<IScheduleEvent> _events;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleBuilder"/> class.
    /// </summary>
    /// <param name="scheduleName">The name of the schedule being built.</param>
    /// <exception cref="ArgumentException">Thrown when scheduleName is null or whitespace.</exception>
    public ScheduleBuilder(string scheduleName)
    {
        if (string.IsNullOrWhiteSpace(scheduleName))
            throw new ArgumentException("Schedule name cannot be null or whitespace.", nameof(scheduleName));

        _scheduleName = scheduleName;
        _events = new List<IScheduleEvent>();
    }

    /// <summary>
    /// Adds a daily schedule event that triggers at the specified time every day.
    /// </summary>
    /// <param name="eventTime">The time when the event should trigger daily (in UTC).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when eventTime is default DateTimeOffset.</exception>
    public ScheduleBuilder AddDaily(DateTimeOffset eventTime, string? data = null, bool isDisabled = false)
    {
        if (eventTime == default)
            throw new ArgumentException("Event time cannot be default DateTimeOffset.", nameof(eventTime));

        var scheduleEvent = new DailyScheduleEvent(eventTime, data)
        {
            IsDisabled = isDisabled
        };

        _events.Add(scheduleEvent);
        return this;
    }

    /// <summary>
    /// Adds a daily schedule event that triggers at the specified time every day.
    /// </summary>
    /// <param name="time">The time of day when the event should trigger (hours, minutes, seconds).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when time is outside valid 24-hour range.</exception>
    public ScheduleBuilder AddDaily(TimeSpan time, string? data = null, bool isDisabled = false)
    {
        if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(time), "Time must be between 00:00:00 and 23:59:59.");

        var eventTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date.Add(time), TimeSpan.Zero);
        return AddDaily(eventTime, data, isDisabled);
    }

    /// <summary>
    /// Adds a daily schedule event that triggers at the specified hour and minute every day.
    /// </summary>
    /// <param name="hour">The hour (0-23) when the event should trigger.</param>
    /// <param name="minute">The minute (0-59) when the event should trigger.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hour or minute are outside valid ranges.</exception>
    public ScheduleBuilder AddDaily(int hour, int minute, string? data = null, bool isDisabled = false)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
        if (minute < 0 || minute > 59)
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59.");

        return AddDaily(new TimeSpan(hour, minute, 0), data, isDisabled);
    }

    /// <summary>
    /// Adds a weekday schedule event that triggers on specific days of the week at the specified time.
    /// </summary>
    /// <param name="eventTime">The time when the event should trigger (in UTC).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when eventTime is default DateTimeOffset or daysOfWeek is null/empty.</exception>
    public ScheduleBuilder AddWeekday(DateTimeOffset eventTime, string? data, DayOfWeek[] daysOfWeek, bool isDisabled = false)
    {
        if (eventTime == default)
            throw new ArgumentException("Event time cannot be default DateTimeOffset.", nameof(eventTime));
        if (daysOfWeek == null || daysOfWeek.Length == 0)
            throw new ArgumentException("Days of week cannot be null or empty.", nameof(daysOfWeek));

        var scheduleEvent = new WeekdayScheduleEvent(eventTime, data, daysOfWeek)
        {
            IsDisabled = isDisabled
        };

        _events.Add(scheduleEvent);
        return this;
    }

    /// <summary>
    /// Adds a weekday schedule event that triggers on specific days of the week at the specified time.
    /// </summary>
    /// <param name="time">The time of day when the event should trigger (hours, minutes, seconds).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when time is outside valid 24-hour range.</exception>
    /// <exception cref="ArgumentException">Thrown when daysOfWeek is null/empty.</exception>
    public ScheduleBuilder AddWeekday(TimeSpan time, string? data, DayOfWeek[] daysOfWeek, bool isDisabled = false)
    {
        if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            throw new ArgumentOutOfRangeException(nameof(time), "Time must be between 00:00:00 and 23:59:59.");

        var eventTime = new DateTimeOffset(DateTimeOffset.UtcNow.Date.Add(time), TimeSpan.Zero);
        return AddWeekday(eventTime, data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a weekday schedule event that triggers on specific days of the week at the specified hour and minute.
    /// </summary>
    /// <param name="hour">The hour (0-23) when the event should trigger.</param>
    /// <param name="minute">The minute (0-59) when the event should trigger.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when hour or minute are outside valid ranges.</exception>
    /// <exception cref="ArgumentException">Thrown when daysOfWeek is null/empty.</exception>
    public ScheduleBuilder AddWeekday(int hour, int minute, string? data, DayOfWeek[] daysOfWeek, bool isDisabled = false)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
        if (minute < 0 || minute > 59)
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59.");

        return AddWeekday(new TimeSpan(hour, minute, 0), data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a weekday schedule event that triggers on weekdays (Monday through Friday) at the specified time.
    /// </summary>
    /// <param name="time">The time of day when the event should trigger (hours, minutes, seconds).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when time is outside valid 24-hour range.</exception>
    public ScheduleBuilder AddWeekdays(TimeSpan time, string? data = null, bool isDisabled = false)
    {
        var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        return AddWeekday(time, data, weekdays, isDisabled);
    }

    /// <summary>
    /// Adds a weekday schedule event that triggers on weekends (Saturday and Sunday) at the specified time.
    /// </summary>
    /// <param name="time">The time of day when the event should trigger (hours, minutes, seconds).</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when time is outside valid 24-hour range.</exception>
    public ScheduleBuilder AddWeekends(TimeSpan time, string? data = null, bool isDisabled = false)
    {
        var weekends = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
        return AddWeekday(time, data, weekends, isDisabled);
    }

    /// <summary>
    /// Adds a sunrise offset schedule event that triggers at a specific time offset from sunrise.
    /// </summary>
    /// <param name="offset">The time offset from sunrise when this event should trigger. 
    /// Positive values represent time after sunrise, negative values represent time before sunrise.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder AddSunriseOffset(TimeSpan offset, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        var scheduleEvent = new SunriseOffsetScheduleEvent(offset, data, daysOfWeek)
        {
            IsDisabled = isDisabled
        };

        _events.Add(scheduleEvent);
        return this;
    }

    /// <summary>
    /// Adds a sunrise offset schedule event that triggers a specified number of minutes before or after sunrise.
    /// </summary>
    /// <param name="offsetMinutes">The number of minutes from sunrise when this event should trigger. 
    /// Positive values represent time after sunrise, negative values represent time before sunrise.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder AddSunriseOffset(int offsetMinutes, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return AddSunriseOffset(TimeSpan.FromMinutes(offsetMinutes), data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a sunset offset schedule event that triggers at a specific time offset from sunset.
    /// </summary>
    /// <param name="offset">The time offset from sunset when this event should trigger. 
    /// Positive values represent time after sunset, negative values represent time before sunset.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder AddSunsetOffset(TimeSpan offset, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        var scheduleEvent = new SunsetOffsetScheduleEvent(offset, data, daysOfWeek)
        {
            IsDisabled = isDisabled
        };

        _events.Add(scheduleEvent);
        return this;
    }

    /// <summary>
    /// Adds a sunset offset schedule event that triggers a specified number of minutes before or after sunset.
    /// </summary>
    /// <param name="offsetMinutes">The number of minutes from sunset when this event should trigger. 
    /// Positive values represent time after sunset, negative values represent time before sunset.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder AddSunsetOffset(int offsetMinutes, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return AddSunsetOffset(TimeSpan.FromMinutes(offsetMinutes), data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds an existing schedule event to the builder.
    /// </summary>
    /// <param name="scheduleEvent">The schedule event to add.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scheduleEvent is null.</exception>
    public ScheduleBuilder AddEvent(IScheduleEvent scheduleEvent)
    {
        if (scheduleEvent == null)
            throw new ArgumentNullException(nameof(scheduleEvent));

        _events.Add(scheduleEvent);
        return this;
    }

    /// <summary>
    /// Adds multiple existing schedule events to the builder.
    /// </summary>
    /// <param name="scheduleEvents">The schedule events to add.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when scheduleEvents is null.</exception>
    /// <exception cref="ArgumentException">Thrown when scheduleEvents contains null items.</exception>
    public ScheduleBuilder AddEvents(IEnumerable<IScheduleEvent> scheduleEvents)
    {
        if (scheduleEvents == null)
            throw new ArgumentNullException(nameof(scheduleEvents));

        var eventList = scheduleEvents.ToList();
        if (eventList.Any(e => e == null))
            throw new ArgumentException("Schedule events cannot contain null items.", nameof(scheduleEvents));

        _events.AddRange(eventList);
        return this;
    }

    /// <summary>
    /// Removes all events of the specified type from the builder.
    /// </summary>
    /// <param name="eventType">The type of events to remove.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder RemoveEventsOfType(ScheduleEventType eventType)
    {
        _events.RemoveAll(e => e.EventType == eventType);
        return this;
    }

    /// <summary>
    /// Removes all events from the builder.
    /// </summary>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public ScheduleBuilder ClearEvents()
    {
        _events.Clear();
        return this;
    }

    /// <summary>
    /// Gets the current number of events in the builder.
    /// </summary>
    /// <returns>The number of events currently added to the builder.</returns>
    public int EventCount => _events.Count;

    /// <summary>
    /// Gets a read-only collection of the events currently in the builder.
    /// </summary>
    /// <returns>A read-only collection of the current events.</returns>
    public IReadOnlyList<IScheduleEvent> Events => _events.AsReadOnly();

    /// <summary>
    /// Builds and returns the completed <see cref="Schedule"/> object.
    /// </summary>
    /// <returns>A new <see cref="Schedule"/> instance with the configured name and events.</returns>
    public Schedule Build()
    {
        return new Schedule
        {
            Name = _scheduleName,
            Events = new List<IScheduleEvent>(_events)
        };
    }

    /// <summary>
    /// Creates a new <see cref="ScheduleBuilder"/> instance with the specified name.
    /// </summary>
    /// <param name="scheduleName">The name of the schedule being built.</param>
    /// <returns>A new <see cref="ScheduleBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when scheduleName is null or whitespace.</exception>
    public static ScheduleBuilder Create(string scheduleName)
    {
        return new ScheduleBuilder(scheduleName);
    }
}

/// <summary>
/// Provides extension methods for creating common schedule patterns.
/// </summary>
public static class ScheduleBuilderExtensions
{
    /// <summary>
    /// Adds a schedule event that triggers at sunrise on the specified days.
    /// </summary>
    /// <param name="builder">The schedule builder instance.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public static ScheduleBuilder AddSunrise(this ScheduleBuilder builder, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return builder.AddSunriseOffset(TimeSpan.Zero, data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a schedule event that triggers at sunset on the specified days.
    /// </summary>
    /// <param name="builder">The schedule builder instance.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public static ScheduleBuilder AddSunset(this ScheduleBuilder builder, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return builder.AddSunsetOffset(TimeSpan.Zero, data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a schedule event that triggers 30 minutes before sunrise on the specified days.
    /// </summary>
    /// <param name="builder">The schedule builder instance.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public static ScheduleBuilder AddBeforeSunrise(this ScheduleBuilder builder, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return builder.AddSunriseOffset(TimeSpan.FromMinutes(-30), data, daysOfWeek, isDisabled);
    }

    /// <summary>
    /// Adds a schedule event that triggers 30 minutes after sunset on the specified days.
    /// </summary>
    /// <param name="builder">The schedule builder instance.</param>
    /// <param name="data">Optional data to be passed when this event triggers.</param>
    /// <param name="daysOfWeek">The days of the week when this event should be active. If null, the event is active every day.</param>
    /// <param name="isDisabled">Whether this event should be disabled initially. Default is false.</param>
    /// <returns>The current <see cref="ScheduleBuilder"/> instance for method chaining.</returns>
    public static ScheduleBuilder AddAfterSunset(this ScheduleBuilder builder, string? data = null, DayOfWeek[]? daysOfWeek = null, bool isDisabled = false)
    {
        return builder.AddSunsetOffset(TimeSpan.FromMinutes(30), data, daysOfWeek, isDisabled);
    }
}