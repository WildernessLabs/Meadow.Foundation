namespace Meadow.Foundation.Scheduling.Tests;

/// <summary>
/// Unit tests for the ScheduleBuilder class.
/// </summary>
public class ScheduleBuilderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidName_CreatesBuilder()
    {
        // Act
        var builder = new ScheduleBuilder("Test Schedule");

        // Assert
        Assert.Equal(0, builder.EventCount);
        Assert.Empty(builder.Events);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ScheduleBuilder(invalidName));
    }

    [Fact]
    public void Create_StaticMethod_CreatesBuilder()
    {
        // Act
        var builder = ScheduleBuilder.Create("Static Test");

        // Assert
        Assert.Equal(0, builder.EventCount);
        Assert.Empty(builder.Events);
    }

    #endregion

    #region AddDaily Tests

    [Fact]
    public void AddDaily_WithDateTime_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var eventTime = new DateTime(2024, 1, 1, 18, 30, 0);

        // Act
        var result = builder.AddDaily(eventTime, "TEST_DATA");

        // Assert
        Assert.Same(builder, result); // Fluent API
        Assert.Equal(1, builder.EventCount);

        var schedule = builder.Build();
        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.Equal(eventTime, dailyEvent.EventTime);
        Assert.Equal("TEST_DATA", dailyEvent.Data);
        Assert.False(dailyEvent.IsDisabled);
    }

    [Fact]
    public void AddDaily_WithTimeSpan_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var eventTime = new TimeSpan(14, 30, 0);

        // Act
        var result = builder.AddDaily(eventTime, "TIMESPAN_DATA");

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(1, builder.EventCount);

        var schedule = builder.Build();
        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.Equal(eventTime, dailyEvent.EventTime.TimeOfDay);
        Assert.Equal("TIMESPAN_DATA", dailyEvent.Data);
    }

    [Fact]
    public void AddDaily_WithHourMinute_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddDaily(9, 15, "HOUR_MINUTE_DATA");

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(1, builder.EventCount);

        var schedule = builder.Build();
        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.Equal(9, dailyEvent.EventTime.Hour);
        Assert.Equal(15, dailyEvent.EventTime.Minute);
        Assert.Equal("HOUR_MINUTE_DATA", dailyEvent.Data);
    }

    [Fact]
    public void AddDaily_WithDisabledFlag_SetsDisabledCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        builder.AddDaily(new DateTime(2024, 1, 1, 12, 0, 0), "DISABLED_DATA", isDisabled: true);

        // Assert
        var schedule = builder.Build();
        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.True(dailyEvent.IsDisabled);
    }

    [Fact]
    public void AddDaily_WithDefaultDateTime_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddDaily(default(DateTimeOffset)));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(24, 0)]
    [InlineData(12, -1)]
    [InlineData(12, 60)]
    public void AddDaily_WithInvalidHourMinute_ThrowsArgumentOutOfRangeException(int hour, int minute)
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddDaily(hour, minute));
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(24, 0, 0)]
    [InlineData(25, 0, 0)] // 25 hours total
    public void AddDaily_WithInvalidTimeSpan_ThrowsArgumentOutOfRangeException(int hours, int minutes, int seconds)
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var invalidTime = new TimeSpan(hours, minutes, seconds);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddDaily(invalidTime));
    }

    #endregion

    #region AddWeekday Tests

    [Fact]
    public void AddWeekday_WithDateTime_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var eventTime = new DateTime(2024, 1, 1, 7, 0, 0);
        var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        // Act
        var result = builder.AddWeekday(eventTime, "WEEKDAY_DATA", daysOfWeek);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(1, builder.EventCount);

        var schedule = builder.Build();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());
        Assert.Equal(eventTime, weekdayEvent.EventTime);
        Assert.Equal("WEEKDAY_DATA", weekdayEvent.Data);
        Assert.Equal(3, weekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, weekdayEvent.DaysOfWeek);
    }

    [Fact]
    public void AddWeekday_WithTimeSpan_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var eventTime = new TimeSpan(16, 45, 0);
        var daysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        // Act
        var result = builder.AddWeekday(eventTime, "WEEKEND_DATA", daysOfWeek);

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());
        Assert.Equal(eventTime, weekdayEvent.EventTime.TimeOfDay);
        Assert.Equal(2, weekdayEvent.DaysOfWeek.Length);
    }

    [Fact]
    public void AddWeekday_WithHourMinute_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var daysOfWeek = new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday };

        // Act
        var result = builder.AddWeekday(8, 30, "HOUR_MINUTE_WEEKDAY", daysOfWeek);

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());
        Assert.Equal(8, weekdayEvent.EventTime.Hour);
        Assert.Equal(30, weekdayEvent.EventTime.Minute);
        Assert.Equal(2, weekdayEvent.DaysOfWeek.Length);
    }

    [Fact]
    public void AddWeekdays_ConvenienceMethod_AddsCorrectDays()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddWeekdays(new TimeSpan(9, 0, 0), "WORKDAYS");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());
        Assert.Equal(5, weekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Tuesday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Thursday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, weekdayEvent.DaysOfWeek);
        Assert.Equal("WORKDAYS", weekdayEvent.Data);
    }

    [Fact]
    public void AddWeekends_ConvenienceMethod_AddsCorrectDays()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddWeekends(new TimeSpan(10, 0, 0), "WEEKEND_TIME");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());
        Assert.Equal(2, weekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, weekdayEvent.DaysOfWeek);
        Assert.Equal("WEEKEND_TIME", weekdayEvent.Data);
    }

    [Fact]
    public void AddWeekday_WithNullDaysOfWeek_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddWeekday(new DateTime(2024, 1, 1, 12, 0, 0), "DATA", null));
    }

    [Fact]
    public void AddWeekday_WithEmptyDaysOfWeek_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            builder.AddWeekday(new DateTime(2024, 1, 1, 12, 0, 0), "DATA", new DayOfWeek[0]));
    }

    #endregion

    #region Sunrise/Sunset Offset Tests

    [Fact]
    public void AddSunriseOffset_WithTimeSpan_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var offset = TimeSpan.FromMinutes(30);
        var daysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        // Act
        var result = builder.AddSunriseOffset(offset, "SUNRISE_DATA", daysOfWeek);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(1, builder.EventCount);

        var schedule = builder.Build();
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(offset, sunriseEvent.Offset);
        Assert.Equal("SUNRISE_DATA", sunriseEvent.Data);
        Assert.Equal(2, sunriseEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, sunriseEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, sunriseEvent.DaysOfWeek);
    }

    [Fact]
    public void AddSunriseOffset_WithMinutes_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddSunriseOffset(-15, "BEFORE_SUNRISE");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.FromMinutes(-15), sunriseEvent.Offset);
        Assert.Equal("BEFORE_SUNRISE", sunriseEvent.Data);
        Assert.Null(sunriseEvent.DaysOfWeek);
    }

    [Fact]
    public void AddSunsetOffset_WithTimeSpan_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var offset = TimeSpan.FromHours(1);

        // Act
        var result = builder.AddSunsetOffset(offset, "SUNSET_DATA");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(offset, sunsetEvent.Offset);
        Assert.Equal("SUNSET_DATA", sunsetEvent.Data);
        Assert.Null(sunsetEvent.DaysOfWeek);
    }

    [Fact]
    public void AddSunsetOffset_WithMinutes_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddSunsetOffset(45, "AFTER_SUNSET", new[] { DayOfWeek.Friday });

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.FromMinutes(45), sunsetEvent.Offset);
        Assert.Single(sunsetEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, sunsetEvent.DaysOfWeek);
    }

    #endregion

    #region Extension Method Tests

    [Fact]
    public void AddSunrise_ExtensionMethod_AddsZeroOffsetEvent()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddSunrise("SUNRISE", new[] { DayOfWeek.Monday });

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.Zero, sunriseEvent.Offset);
        Assert.Equal("SUNRISE", sunriseEvent.Data);
        Assert.Single(sunriseEvent.DaysOfWeek);
    }

    [Fact]
    public void AddSunset_ExtensionMethod_AddsZeroOffsetEvent()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddSunset("SUNSET");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.Zero, sunsetEvent.Offset);
        Assert.Equal("SUNSET", sunsetEvent.Data);
        Assert.Null(sunsetEvent.DaysOfWeek);
    }

    [Fact]
    public void AddBeforeSunrise_ExtensionMethod_AddsNegativeOffsetEvent()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddBeforeSunrise("BEFORE_SUNRISE");

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.FromMinutes(-30), sunriseEvent.Offset);
        Assert.Equal("BEFORE_SUNRISE", sunriseEvent.Data);
    }

    [Fact]
    public void AddAfterSunset_ExtensionMethod_AddsPositiveOffsetEvent()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act
        var result = builder.AddAfterSunset("AFTER_SUNSET", new[] { DayOfWeek.Sunday });

        // Assert
        Assert.Same(builder, result);
        var schedule = builder.Build();
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(schedule.Events.First());
        Assert.Equal(TimeSpan.FromMinutes(30), sunsetEvent.Offset);
        Assert.Single(sunsetEvent.DaysOfWeek);
    }

    #endregion

    #region AddEvent and AddEvents Tests

    [Fact]
    public void AddEvent_WithValidEvent_AddsEventCorrectly()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 15, 0, 0), "EXISTING_EVENT");

        // Act
        var result = builder.AddEvent(dailyEvent);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(1, builder.EventCount);
        var schedule = builder.Build();
        Assert.Same(dailyEvent, schedule.Events.First());
    }

    [Fact]
    public void AddEvent_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddEvent(null));
    }

    [Fact]
    public void AddEvents_WithValidEvents_AddsAllEvents()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var events = new IScheduleEvent[]
        {
            new DailyScheduleEvent(new DateTime(2024, 1, 1, 8, 0, 0), "EVENT1"),
            new DailyScheduleEvent(new DateTime(2024, 1, 1, 20, 0, 0), "EVENT2")
        };

        // Act
        var result = builder.AddEvents(events);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(2, builder.EventCount);
        var schedule = builder.Build();
        Assert.Contains(events[0], schedule.Events);
        Assert.Contains(events[1], schedule.Events);
    }

    [Fact]
    public void AddEvents_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddEvents(null));
    }

    [Fact]
    public void AddEvents_WithNullEvent_ThrowsArgumentException()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        var events = new IScheduleEvent[] { null };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddEvents(events));
    }

    #endregion

    #region Removal and Modification Tests

    [Fact]
    public void RemoveEventsOfType_WithMatchingEvents_RemovesCorrectEvents()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        builder.AddDaily(12, 0, "DAILY1");
        builder.AddDaily(18, 0, "DAILY2");
        builder.AddWeekdays(new TimeSpan(9, 0, 0), "WEEKDAY");
        builder.AddSunrise("SUNRISE");

        // Act
        var result = builder.RemoveEventsOfType(ScheduleEventType.Daily);

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(2, builder.EventCount); // Should have weekday and sunrise left
        var schedule = builder.Build();
        Assert.DoesNotContain(schedule.Events, e => e.EventType == ScheduleEventType.Daily);
        Assert.Contains(schedule.Events, e => e.EventType == ScheduleEventType.Weekday);
        Assert.Contains(schedule.Events, e => e.EventType == ScheduleEventType.SunriseOffset);
    }

    [Fact]
    public void ClearEvents_RemovesAllEvents()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        builder.AddDaily(12, 0, "DAILY");
        builder.AddWeekdays(new TimeSpan(9, 0, 0), "WEEKDAY");
        builder.AddSunrise("SUNRISE");

        // Act
        var result = builder.ClearEvents();

        // Assert
        Assert.Same(builder, result);
        Assert.Equal(0, builder.EventCount);
        Assert.Empty(builder.Events);
    }

    #endregion

    #region Builder Properties Tests

    [Fact]
    public void EventCount_ReflectsCorrectCount()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");

        // Act & Assert
        Assert.Equal(0, builder.EventCount);

        builder.AddDaily(12, 0);
        Assert.Equal(1, builder.EventCount);

        builder.AddWeekdays(new TimeSpan(9, 0, 0));
        Assert.Equal(2, builder.EventCount);

        builder.ClearEvents();
        Assert.Equal(0, builder.EventCount);
    }

    [Fact]
    public void Events_ReturnsReadOnlyCollection()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        builder.AddDaily(12, 0, "TEST");

        // Act
        var events = builder.Events;

        // Assert
        Assert.Single(events);
        Assert.Equal("TEST", events.First().Data);

        // Verify it's read-only by checking type
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyCollection<IScheduleEvent>>(events);
    }

    #endregion

    #region Build Method Tests

    [Fact]
    public void Build_CreatesCorrectSchedule()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test Schedule");
        builder.AddDaily(8, 0, "MORNING");
        builder.AddWeekdays(new TimeSpan(17, 30, 0), "EVENING");

        // Act
        var schedule = builder.Build();

        // Assert
        Assert.Equal("Test Schedule", schedule.Name);
        Assert.Equal(2, schedule.Events.Count);

        var dailyEvent = schedule.Events.OfType<DailyScheduleEvent>().Single();
        Assert.Equal("MORNING", dailyEvent.Data);

        var weekdayEvent = schedule.Events.OfType<WeekdayScheduleEvent>().Single();
        Assert.Equal("EVENING", weekdayEvent.Data);
        Assert.Equal(5, weekdayEvent.DaysOfWeek.Length);
    }

    [Fact]
    public void Build_CreatesNewEventsList()
    {
        // Arrange
        var builder = new ScheduleBuilder("Test");
        builder.AddDaily(12, 0, "TEST");

        // Act
        var schedule1 = builder.Build();
        var schedule2 = builder.Build();

        // Assert
        Assert.NotSame(schedule1.Events, schedule2.Events); // Different list instances
        Assert.Equal(schedule1.Events.Count, schedule2.Events.Count); // Same content
    }

    #endregion

    #region Fluent API Integration Tests

    [Fact]
    public void FluentAPI_ComplexChaining_WorksCorrectly()
    {
        // Act
        var schedule = ScheduleBuilder.Create("Complex Schedule")
            .AddDaily(6, 0, "WAKE_UP")
            .AddWeekdays(new TimeSpan(8, 30, 0), "WORK_START")
            .AddWeekdays(new TimeSpan(17, 0, 0), "WORK_END")
            .AddWeekends(new TimeSpan(9, 0, 0), "WEEKEND_WAKE")
            .AddSunrise("SUNRISE_EVENT", new[] { DayOfWeek.Sunday })
            .AddSunset("SUNSET_EVENT")
            .AddBeforeSunrise("EARLY_BIRD")
            .AddAfterSunset("NIGHT_OWL", new[] { DayOfWeek.Friday, DayOfWeek.Saturday })
            .Build();

        // Assert
        Assert.Equal("Complex Schedule", schedule.Name);
        Assert.Equal(8, schedule.Events.Count);

        // Verify event types
        Assert.Equal(1, schedule.Events.Count(e => e.EventType == ScheduleEventType.Daily));
        Assert.Equal(3, schedule.Events.Count(e => e.EventType == ScheduleEventType.Weekday));
        Assert.Equal(2, schedule.Events.Count(e => e.EventType == ScheduleEventType.SunriseOffset));
        Assert.Equal(2, schedule.Events.Count(e => e.EventType == ScheduleEventType.SunsetOffset));

        // Verify sunrise/sunset detection
        Assert.True(schedule.ContainsSunriseOrSunsetEvents);
    }

    [Fact]
    public void FluentAPI_BuilderReuse_MaintainsState()
    {
        // Arrange
        var builder = ScheduleBuilder.Create("Reusable Builder")
            .AddDaily(12, 0, "NOON");

        // Act
        var schedule1 = builder.Build();
        builder.AddDaily(18, 0, "EVENING");
        var schedule2 = builder.Build();

        // Assert
        Assert.Equal(1, schedule1.Events.Count);
        Assert.Equal(2, schedule2.Events.Count);

        // Original schedule should be unaffected
        Assert.Equal(1, schedule1.Events.Count);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Builder_WithNoEvents_CreatesEmptySchedule()
    {
        // Arrange
        var builder = new ScheduleBuilder("Empty Schedule");

        // Act
        var schedule = builder.Build();

        // Assert
        Assert.Equal("Empty Schedule", schedule.Name);
        Assert.Empty(schedule.Events);
        Assert.False(schedule.ContainsSunriseOrSunsetEvents);
    }

    [Fact]
    public void Builder_WithOnlySolarEvents_DetectsSolarEventsCorrectly()
    {
        // Arrange & Act
        var schedule = ScheduleBuilder.Create("Solar Schedule")
            .AddSunrise("SUNRISE")
            .AddSunset("SUNSET")
            .Build();

        // Assert
        Assert.True(schedule.ContainsSunriseOrSunsetEvents);
    }

    [Fact]
    public void Builder_WithMixedEvents_HandlesDisabledEventsCorrectly()
    {
        // Arrange & Act
        var schedule = ScheduleBuilder.Create("Mixed Schedule")
            .AddDaily(8, 0, "ENABLED", isDisabled: false)
            .AddDaily(20, 0, "DISABLED", isDisabled: true)
            .Build();

        // Assert
        Assert.Equal(2, schedule.Events.Count);
        Assert.Equal(1, schedule.Events.Count(e => e.IsDisabled));
        Assert.Equal(1, schedule.Events.Count(e => !e.IsDisabled));
    }

    #endregion
}