namespace Meadow.Foundation.Scheduling.Tests;

/// <summary>
/// Unit tests for the ScheduleService class.
/// </summary>
public class ScheduleServiceTests : IDisposable
{
    private readonly TestTimeProvider _timeProvider;
    private readonly ScheduleService _scheduleService;
    private readonly List<ScheduleEventTriggeredEventArgs> _triggeredEvents;

    public ScheduleServiceTests()
    {
        _timeProvider = new TestTimeProvider();
        _scheduleService = new ScheduleService(_timeProvider);
        _triggeredEvents = new List<ScheduleEventTriggeredEventArgs>();

        _scheduleService.ScheduleEventTriggered += (sender, args) => _triggeredEvents.Add(args);
    }

    public void Dispose()
    {
        _scheduleService?.Dispose();
    }

    #region Basic Functionality Tests

    [Fact]
    public async Task SetSchedules_WithValidCollection_SetsSuccessfully()
    {
        // Arrange
        var schedule = new Schedule { Name = "Test Schedule" };
        var collection = new ScheduleCollection(new[] { schedule });

        // Act
        await _scheduleService.SetSchedules(collection);

        // Assert - No exception should be thrown
        Assert.False(_scheduleService.IsDisposed);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithNoSchedules_CompletesSuccessfully()
    {
        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithEmptySchedule_CompletesSuccessfully()
    {
        // Arrange
        var schedule = new Schedule { Name = "Empty Schedule" };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    #endregion

    #region Daily Event Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithDailyEventAtCurrentTime_TriggersEvent()
    {
        // Arrange - Current time is 12:00 noon
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "NOON_EVENT");
        var schedule = new Schedule
        {
            Name = "Daily Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("Daily Test", triggeredEvent.Name);
        Assert.Equal("NOON_EVENT", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.Daily, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithDailyEventAtDifferentTime_DoesNotTrigger()
    {
        // Arrange - Current time is 12:00, event at 15:00
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 15, 0, 0), "AFTERNOON_EVENT");
        var schedule = new Schedule
        {
            Name = "Daily Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithDisabledDailyEvent_DoesNotTrigger()
    {
        // Arrange
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "DISABLED_EVENT")
        {
            IsDisabled = true
        };
        var schedule = new Schedule
        {
            Name = "Daily Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    #endregion

    #region Weekday Event Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithWeekdayEventOnSaturday_TriggersEvent()
    {
        // Arrange - Current time is Saturday 12:00
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 12, 0, 0),
            "WEEKEND_EVENT",
            new[] { DayOfWeek.Saturday, DayOfWeek.Sunday });
        var schedule = new Schedule
        {
            Name = "Weekday Test",
            Events = { weekdayEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("Weekday Test", triggeredEvent.Name);
        Assert.Equal("WEEKEND_EVENT", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.Weekday, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithWeekdayEventOnWrongDay_DoesNotTrigger()
    {
        // Arrange - Current time is Saturday, but event is for weekdays
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 12, 0, 0),
            "WEEKDAY_EVENT",
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });
        var schedule = new Schedule
        {
            Name = "Weekday Test",
            Events = { weekdayEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithWeekdayEventWrongTime_DoesNotTrigger()
    {
        // Arrange - Saturday at correct day but wrong time
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 15, 0, 0),
            "WEEKEND_EVENT",
            new[] { DayOfWeek.Saturday });
        var schedule = new Schedule
        {
            Name = "Weekday Test",
            Events = { weekdayEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    #endregion

    #region Sunrise Offset Event Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithSunriseOffsetEventAtCorrectTime_TriggersEvent()
    {
        // Arrange - Sunrise is 6:30, offset +5:30 = 12:00 (current time)
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromHours(5.5), // 5 hours 30 minutes after sunrise
            "SUNRISE_OFFSET_EVENT");
        var schedule = new Schedule
        {
            Name = "Sunrise Test",
            Events = { sunriseEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("Sunrise Test", triggeredEvent.Name);
        Assert.Equal("SUNRISE_OFFSET_EVENT", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.SunriseOffset, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithSunriseOffsetEventWrongTime_DoesNotTrigger()
    {
        // Arrange - Sunrise is 6:30, offset +2:00 = 8:30 (not current time)
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromHours(2),
            "SUNRISE_OFFSET_EVENT");
        var schedule = new Schedule
        {
            Name = "Sunrise Test",
            Events = { sunriseEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithSunriseOffsetEventOnWrongDay_DoesNotTrigger()
    {
        // Arrange - Correct time but wrong day (current is Saturday)
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromHours(5.5),
            "SUNRISE_OFFSET_EVENT",
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday });
        var schedule = new Schedule
        {
            Name = "Sunrise Test",
            Events = { sunriseEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithNegativeSunriseOffset_CalculatesCorrectly()
    {
        // Arrange - Set time to 5:30 (1 hour before sunrise at 6:30)
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 5, 30, 0, TimeSpan.Zero);
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromHours(-1), // 1 hour before sunrise
            "BEFORE_SUNRISE_EVENT");
        var schedule = new Schedule
        {
            Name = "Sunrise Test",
            Events = { sunriseEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        Assert.Equal("BEFORE_SUNRISE_EVENT", _triggeredEvents.First().Data);
    }

    #endregion

    #region Sunset Offset Event Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithSunsetOffsetEventAtCorrectTime_TriggersEvent()
    {
        // Arrange - Sunset is 18:30, offset -6:30 = 12:00 (current time)
        var sunsetEvent = new SunsetOffsetScheduleEvent(
            TimeSpan.FromHours(-6.5), // 6.5 hours before sunset
            "SUNSET_OFFSET_EVENT");
        var schedule = new Schedule
        {
            Name = "Sunset Test",
            Events = { sunsetEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("Sunset Test", triggeredEvent.Name);
        Assert.Equal("SUNSET_OFFSET_EVENT", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.SunsetOffset, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithPositiveSunsetOffset_CalculatesCorrectly()
    {
        // Arrange - Set time to 19:30 (1 hour after sunset at 18:30)
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 19, 30, 0, TimeSpan.Zero);
        var sunsetEvent = new SunsetOffsetScheduleEvent(
            TimeSpan.FromHours(1), // 1 hour after sunset
            "AFTER_SUNSET_EVENT");
        var schedule = new Schedule
        {
            Name = "Sunset Test",
            Events = { sunsetEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        Assert.Equal("AFTER_SUNSET_EVENT", _triggeredEvents.First().Data);
    }

    #endregion

    #region Priority and Multiple Event Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithMultipleActiveEvents_TriggersAllEvents()
    {
        // Arrange - Multiple events that should all trigger at 12:00 on Saturday
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "DAILY_EVENT");
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 12, 0, 0),
            "WEEKEND_EVENT",
            new[] { DayOfWeek.Saturday, DayOfWeek.Sunday });
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromHours(5.5), // 6:30 + 5:30 = 12:00
            "SUNRISE_EVENT");

        var schedule = new Schedule
        {
            Name = "Multi Event Test",
            Events = { dailyEvent, weekdayEvent, sunriseEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Equal(3, _triggeredEvents.Count);
        Assert.Contains(_triggeredEvents, e => e.Data == "DAILY_EVENT");
        Assert.Contains(_triggeredEvents, e => e.Data == "WEEKEND_EVENT");
        Assert.Contains(_triggeredEvents, e => e.Data == "SUNRISE_EVENT");
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithMixedActiveAndInactive_TriggersOnlyActive()
    {
        // Arrange
        var activeEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "ACTIVE_EVENT");
        var inactiveEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 15, 0, 0), "INACTIVE_EVENT");
        var disabledEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "DISABLED_EVENT")
        {
            IsDisabled = true
        };

        var schedule = new Schedule
        {
            Name = "Mixed Test",
            Events = { activeEvent, inactiveEvent, disabledEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        Assert.Equal("ACTIVE_EVENT", _triggeredEvents.First().Data);
    }

    #endregion

    #region Multiple Schedule Tests

    [Fact]
    public async Task ApplyScheduleAsync_WithMultipleSchedules_TriggersEventsFromAll()
    {
        // Arrange
        var schedule1 = new Schedule
        {
            Name = "Schedule 1",
            Events = { new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "SCHEDULE_1_EVENT") }
        };
        var schedule2 = new Schedule
        {
            Name = "Schedule 2",
            Events = { new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "SCHEDULE_2_EVENT") }
        };
        var collection = new ScheduleCollection(new[] { schedule1, schedule2 });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Equal(2, _triggeredEvents.Count);
        Assert.Contains(_triggeredEvents, e => e.Name == "Schedule 1" && e.Data == "SCHEDULE_1_EVENT");
        Assert.Contains(_triggeredEvents, e => e.Name == "Schedule 2" && e.Data == "SCHEDULE_2_EVENT");
    }

    #endregion

    #region Time Matching Tests

    [Theory]
    [InlineData(12, 0, 0, true)]   // Exact match
    [InlineData(12, 0, 30, true)]  // Same minute
    [InlineData(12, 0, 59, true)]  // Same minute, different second
    [InlineData(12, 1, 0, false)]  // Different minute
    [InlineData(11, 59, 59, false)] // Previous minute
    public async Task ApplyScheduleAsync_TimeMatching_WorksCorrectly(int hour, int minute, int second, bool shouldTrigger)
    {
        // Arrange
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, hour, minute, second, TimeSpan.Zero);
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "TIME_TEST");
        var schedule = new Schedule
        {
            Name = "Time Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        if (shouldTrigger)
        {
            Assert.Single(_triggeredEvents);
            Assert.Equal("TIME_TEST", _triggeredEvents.First().Data);
        }
        else
        {
            Assert.Empty(_triggeredEvents);
        }
    }

    #endregion

    #region Day of Week Tests

    [Theory]
    [InlineData(DayOfWeek.Monday, 10, false)]    // Monday June 10, 2024
    [InlineData(DayOfWeek.Tuesday, 11, false)]   // Tuesday June 11, 2024  
    [InlineData(DayOfWeek.Wednesday, 12, false)] // Wednesday June 12, 2024
    [InlineData(DayOfWeek.Thursday, 13, false)]  // Thursday June 13, 2024
    [InlineData(DayOfWeek.Friday, 14, false)]    // Friday June 14, 2024
    [InlineData(DayOfWeek.Saturday, 15, true)]   // Saturday June 15, 2024 (current)
    [InlineData(DayOfWeek.Sunday, 16, false)]    // Sunday June 16, 2024
    public async Task ApplyScheduleAsync_DayOfWeekFiltering_WorksCorrectly(DayOfWeek targetDay, int dayOfMonth, bool shouldTrigger)
    {
        // Arrange - June 15, 2024 is a Saturday
        _timeProvider.Now = new DateTimeOffset(2024, 6, dayOfMonth, 12, 0, 0, TimeSpan.Zero);
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 12, 0, 0),
            "DAY_TEST",
            new[] { DayOfWeek.Saturday }); // Only Saturday
        var schedule = new Schedule
        {
            Name = "Day Test",
            Events = { weekdayEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        if (shouldTrigger)
        {
            Assert.Single(_triggeredEvents);
            Assert.Equal("DAY_TEST", _triggeredEvents.First().Data);
        }
        else
        {
            Assert.Empty(_triggeredEvents);
        }
    }
    #endregion

    #region Disposal Tests

    [Fact]
    public async Task ApplyScheduleAsync_AfterDisposal_DoesNotThrow()
    {
        // Arrange
        var schedule = new Schedule { Name = "Test" };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        _scheduleService.Dispose();

        // Assert
        Assert.True(_scheduleService.IsDisposed);
        // Should not throw
        await _scheduleService.ApplyScheduleAsync();
        Assert.Empty(_triggeredEvents);
    }

    #endregion

    #region Synchronous ApplySchedule Tests

    [Fact]
    public void ApplySchedule_SynchronousVersion_WorksCorrectly()
    {
        // Arrange
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "SYNC_EVENT");
        var schedule = new Schedule
        {
            Name = "Sync Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        _scheduleService.SetSchedules(collection).Wait();

        // Act
        _scheduleService.ApplySchedule(); // Synchronous call

        // Assert
        Assert.Single(_triggeredEvents);
        Assert.Equal("SYNC_EVENT", _triggeredEvents.First().Data);
    }

    #endregion

    #region Event Arguments Tests

    [Fact]
    public async Task ScheduleEventTriggered_IncludesCorrectEventArgs()
    {
        // Arrange
        var testTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.Now = testTime;

        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "TEST_DATA");
        var schedule = new Schedule
        {
            Name = "Event Args Test",
            Events = { dailyEvent }
        };
        var collection = new ScheduleCollection(new[] { schedule });
        await _scheduleService.SetSchedules(collection);

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var eventArgs = _triggeredEvents.First();

        Assert.Equal("Event Args Test", eventArgs.Name);
        Assert.Equal("TEST_DATA", eventArgs.Data);
        Assert.Equal(testTime, eventArgs.TriggeredAt);
        Assert.Equal(dailyEvent, eventArgs.ScheduleEvent);
        Assert.Equal(ScheduleEventType.Daily, eventArgs.ScheduleEvent.EventType);
    }

    #endregion

    #region Integration Tests with Real Schedule Data

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case 1: Saturday 18:00 - Should trigger light events
        // Light Daily 18:00 + Light SunsetOffset -30min (18:30-0:30=18:00) both on Saturday
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 18, 0, 0, TimeSpan.Zero); // Saturday 6 PM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Equal(2, _triggeredEvents.Count);
        Assert.Contains(_triggeredEvents, e => e.Name == "light" && e.Data == "true" &&
            e.ScheduleEvent.EventType == ScheduleEventType.Daily);
        Assert.Contains(_triggeredEvents, e => e.Name == "light" && e.Data == "true" &&
            e.ScheduleEvent.EventType == ScheduleEventType.SunsetOffset);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_SunriseOffset_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Saturday 6:45 - Should trigger fountain sunrise offset
        // Fountain SunriseOffset +15min (6:30+0:15=6:45) on Saturday
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 6, 45, 0, TimeSpan.Zero); // Saturday 6:45 AM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("fountain", triggeredEvent.Name);
        Assert.Equal("true", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.SunriseOffset, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_WeekdayEvents_WorkCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Monday 17:30 - Should trigger light weekday event
        _timeProvider.Now = new DateTimeOffset(2024, 6, 10, 17, 30, 0, TimeSpan.Zero); // Monday 5:30 PM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("light", triggeredEvent.Name);
        Assert.Equal("true", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.Weekday, triggeredEvent.ScheduleEvent.EventType);

        // Verify it's the correct weekday event
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(triggeredEvent.ScheduleEvent);
        Assert.Contains(DayOfWeek.Monday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, weekdayEvent.DaysOfWeek);
        Assert.Equal(5, weekdayEvent.DaysOfWeek.Length); // Mon-Fri
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_FountainWeekdayEvents_WorkCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Wednesday 6:30 - Should trigger fountain weekday event (Mon/Wed/Fri)
        _timeProvider.Now = new DateTimeOffset(2024, 6, 12, 6, 30, 0, TimeSpan.Zero); // Wednesday 6:30 AM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("fountain", triggeredEvent.Name);
        Assert.Equal("true", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.Weekday, triggeredEvent.ScheduleEvent.EventType);

        // Verify it's the correct weekday event
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(triggeredEvent.ScheduleEvent);
        Assert.Contains(DayOfWeek.Wednesday, weekdayEvent.DaysOfWeek);
        Assert.Equal(3, weekdayEvent.DaysOfWeek.Length); // Mon/Wed/Fri
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_DisabledEvents_DoNotTrigger()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Saturday 7:30 - Should trigger disabled light sunrise offset event time
        // Light SunriseOffset +1hr (6:30+1:00=7:30) but it's DISABLED
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 7, 30, 0, TimeSpan.Zero); // Saturday 7:30 AM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert - No events should trigger because the sunrise offset event is disabled
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_MultipleSchedulesTrigger()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: 8:00 AM - Should trigger fountain daily event
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.Zero); // Saturday 8:00 AM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("fountain", triggeredEvent.Name);
        Assert.Equal("true", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.Daily, triggeredEvent.ScheduleEvent.EventType);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_SunsetOffsetAllDays_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Tuesday 20:30 - Should trigger fountain sunset offset (all days)
        // Fountain SunsetOffset +2hrs (18:30+2:00=20:30) on any day
        _timeProvider.Now = new DateTimeOffset(2024, 6, 11, 20, 30, 0, TimeSpan.Zero); // Tuesday 8:30 PM
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Single(_triggeredEvents);
        var triggeredEvent = _triggeredEvents.First();
        Assert.Equal("fountain", triggeredEvent.Name);
        Assert.Equal("false", triggeredEvent.Data);
        Assert.Equal(ScheduleEventType.SunsetOffset, triggeredEvent.ScheduleEvent.EventType);

        // Verify the event has null days (meaning all days)
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(triggeredEvent.ScheduleEvent);
        Assert.Null(sunsetEvent.DaysOfWeek);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_NoEventsAtNoon_ReturnsEmpty()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Test Case: Saturday 12:00 noon - No events should trigger at this time
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday noon
        _triggeredEvents.Clear();

        // Act
        await _scheduleService.ApplyScheduleAsync();

        // Assert
        Assert.Empty(_triggeredEvents);
    }

    [Fact]
    public async Task ApplyScheduleAsync_WithTestSchedule1Json_VerifyScheduleProperties()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        await _scheduleService.SetSchedules(scheduleCollection);

        // Act & Assert - Verify the schedule collection properties
        Assert.True(scheduleCollection.ContainsSunriseOrSunsetEvents);
        Assert.Equal(2, scheduleCollection.Count);

        // Verify timezone information
        Assert.NotNull(scheduleCollection.Timezone);
        Assert.Equal("America/Chicago", scheduleCollection.Timezone.TimezoneName);
        Assert.Equal(-6.0, scheduleCollection.Timezone.UtcOffsetHours);
        Assert.NotNull(scheduleCollection.Timezone.DaylightSavingTime);

        var lightSchedule = scheduleCollection["light"]!;
        var fountainSchedule = scheduleCollection["fountain"]!;

        Assert.True(lightSchedule.ContainsSunriseOrSunsetEvents);
        Assert.True(fountainSchedule.ContainsSunriseOrSunsetEvents);

        Assert.Equal(5, lightSchedule.Events.Count);
        Assert.Equal(5, fountainSchedule.Events.Count);

        // Verify that one event is disabled in light schedule
        Assert.Equal(1, lightSchedule.Events.Count(e => e.IsDisabled));
        Assert.Equal(0, fountainSchedule.Events.Count(e => e.IsDisabled));
    }

    #endregion
}