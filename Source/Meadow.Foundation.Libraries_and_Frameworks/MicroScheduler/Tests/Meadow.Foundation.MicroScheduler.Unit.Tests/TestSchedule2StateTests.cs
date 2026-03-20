namespace Meadow.Foundation.Scheduling.Tests;

/// <summary>
/// Unit tests for test_schedule_2.json that verify state data at 15 minutes past each hour.
/// </summary>
public class TestSchedule2StateTests : IDisposable
{
    private readonly TestTimeProvider _timeProvider;
    private readonly ScheduleService _scheduleService;
    private readonly ScheduleCollection _scheduleCollection;

    public TestSchedule2StateTests()
    {
        _timeProvider = new TestTimeProvider();
        _scheduleService = new ScheduleService(_timeProvider);
        
        // Load test_schedule_2.json
        var filePath = Path.Combine("inputs", "test_schedule_2.json");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");
        
        var json = File.ReadAllText(filePath);
        _scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(json);
        _scheduleService.SetSchedules(_scheduleCollection).Wait();
    }

    public void Dispose()
    {
        _scheduleService?.Dispose();
    }


    [Theory]
    // Early morning hours - lights carry over from previous day's 23:00=false, fountain alternates
    [InlineData(0, 15, "false", "false")]  // 00:15 - lights=false (from prev day 23:00), fountain=false (from prev day 23:30)
    [InlineData(1, 15, "false", "true")]   // 01:15 - lights=false (from prev day 23:00), fountain=true (from 00:30)
    [InlineData(2, 15, "false", "false")]  // 02:15 - lights=false (from prev day 23:00), fountain=false (from 01:30)
    [InlineData(3, 15, "false", "true")]   // 03:15 - lights=false (from prev day 23:00), fountain=true (from 02:30)
    [InlineData(4, 15, "false", "false")]  // 04:15 - lights=false (from prev day 23:00), fountain=false (from 03:30)
    [InlineData(5, 15, "false", "true")]   // 05:15 - lights=false (from prev day 23:00), fountain=true (from 04:30)
    [InlineData(6, 15, "false", "false")]  // 06:15 - lights=false (from prev day 23:00), fountain=false (from 05:30)
    [InlineData(7, 15, "false", "true")]   // 07:15 - lights=false (from prev day 23:00), fountain=true (from 06:30)
    // When lights schedule starts at 08:00
    [InlineData(8, 15, "true", "false")]   // 08:15 - lights=true (from 08:00), fountain=false (from 07:30)
    [InlineData(9, 15, "false", "true")]   // 09:15 - lights=false (from 09:00), fountain=true (from 08:30)
    [InlineData(10, 15, "true", "false")]  // 10:15 - lights=true (from 10:00), fountain=false (from 09:30)
    [InlineData(11, 15, "false", "true")]  // 11:15 - lights=false (from 11:00), fountain=true (from 10:30)
    [InlineData(12, 15, "true", "false")]  // 12:15 - lights=true (from 12:00), fountain=false (from 11:30)
    [InlineData(13, 15, "false", "true")]  // 13:15 - lights=false (from 13:00), fountain=true (from 12:30)
    [InlineData(14, 15, "true", "false")]  // 14:15 - lights=true (from 14:00), fountain=false (from 13:30)
    [InlineData(15, 15, "false", "true")]  // 15:15 - lights=false (from 15:00), fountain=true (from 14:30)
    [InlineData(16, 15, "true", "false")]  // 16:15 - lights=true (from 16:00), fountain=false (from 15:30)
    [InlineData(17, 15, "false", "true")]  // 17:15 - lights=false (from 17:00), fountain=true (from 16:30)
    [InlineData(18, 15, "true", "false")]  // 18:15 - lights=true (from 18:00), fountain=false (from 17:30)
    [InlineData(19, 15, "false", "true")]  // 19:15 - lights=false (from 19:00), fountain=true (from 18:30)
    [InlineData(20, 15, "true", "false")]  // 20:15 - lights=true (from 20:00), fountain=false (from 19:30)
    [InlineData(21, 15, "false", "true")]  // 21:15 - lights=false (from 21:00), fountain=true (from 20:30)
    [InlineData(22, 15, "true", "false")]  // 22:15 - lights=true (from 22:00), fountain=false (from 21:30)
    [InlineData(23, 15, "false", "true")]  // 23:15 - lights=false (from 23:00), fountain=true (from 22:30)
    public void VerifyStateAt15MinutesPastHour(int hour, int minute, string? expectedLightsState, string? expectedFountainState)
    {
        // Arrange - Set time to 15 minutes past the specified hour on a test date
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, hour, minute, 0, TimeSpan.Zero);

        // Get the current state for both schedules based on the most recently triggered events
        var lightsSchedule = _scheduleCollection["lights"]!;
        var fountainSchedule = _scheduleCollection["fountain"]!;
        
        var sunTimes = _timeProvider.GetUtcSunriseAndSunset().GetAwaiter().GetResult();
        
        // Act - Get the current state by finding the most recent active event
        var lightsState = GetCurrentScheduleState(lightsSchedule, _timeProvider.Now, sunTimes);
        var fountainState = GetCurrentScheduleState(fountainSchedule, _timeProvider.Now, sunTimes);

        // Assert
        Assert.Equal(expectedLightsState, lightsState);
        Assert.Equal(expectedFountainState, fountainState);
    }

    /// <summary>
    /// Gets the current state of a schedule by using the corrected GetActiveEvent method.
    /// </summary>
    /// <param name="schedule">The schedule to evaluate.</param>
    /// <param name="currentTime">The current time to evaluate at.</param>
    /// <param name="sunTimes">Sunrise and sunset times for sun-based events.</param>
    /// <returns>The current state data, or null if no events have occurred.</returns>
    private string? GetCurrentScheduleState(Schedule schedule, DateTimeOffset currentTime, (DateTimeOffset Sunrise, DateTimeOffset Sunset) sunTimes)
    {
        // Now we can use the corrected GetActiveEvent method
        var activeEvent = schedule.GetActiveEvent(currentTime, sunTimes);
        return activeEvent?.Data;
    }



    [Fact]
    public void TestSchedule2_VerifyScheduleStructure()
    {
        // Verify the schedule was loaded correctly
        Assert.Equal(2, _scheduleCollection.Count);
        
        var lightsSchedule = _scheduleCollection["lights"]!;
        var fountainSchedule = _scheduleCollection["fountain"]!;
        
        Assert.NotNull(lightsSchedule);
        Assert.NotNull(fountainSchedule);
        
        // Lights schedule: 16 events from 08:00 to 23:00 (alternating true/false every hour)
        Assert.Equal(16, lightsSchedule.Events.Count);
        Assert.All(lightsSchedule.Events, e => Assert.Equal(ScheduleEventType.Daily, e.EventType));
        Assert.All(lightsSchedule.Events, e => Assert.False(e.IsDisabled));
        
        // Fountain schedule: 24 events from 00:30 to 23:30 (alternating true/false every hour at :30)
        Assert.Equal(24, fountainSchedule.Events.Count);
        Assert.All(fountainSchedule.Events, e => Assert.Equal(ScheduleEventType.Daily, e.EventType));
        Assert.All(fountainSchedule.Events, e => Assert.False(e.IsDisabled));
        
        // Verify timezone
        Assert.NotNull(_scheduleCollection.Timezone);
        Assert.Equal("America/Chicago", _scheduleCollection.Timezone.TimezoneName);
        Assert.Equal(-6.0, _scheduleCollection.Timezone.UtcOffsetHours);
    }

    [Fact]
    public void TestSchedule2_VerifyLightsEventPattern()
    {
        // Verify lights events alternate true/false starting at 08:00
        var lightsSchedule = _scheduleCollection["lights"]!;
        var lightsEvents = lightsSchedule.Events.Cast<DailyScheduleEvent>().OrderBy(e => e.EventTime).ToList();
        
        for (int i = 0; i < lightsEvents.Count; i++)
        {
            var expectedHour = 8 + i; // Start at 8:00
            var expectedState = (i % 2 == 0) ? "true" : "false"; // Alternate: true, false, true, false...
            
            Assert.Equal(expectedHour, lightsEvents[i].EventTime.Hour);
            Assert.Equal(0, lightsEvents[i].EventTime.Minute);
            Assert.Equal(expectedState, lightsEvents[i].Data);
        }
    }

    [Fact]
    public void TestSchedule2_VerifyFountainEventPattern()
    {
        // Verify fountain events alternate true/false starting at 00:30
        var fountainSchedule = _scheduleCollection["fountain"]!;
        var fountainEvents = fountainSchedule.Events.Cast<DailyScheduleEvent>().OrderBy(e => e.EventTime).ToList();
        
        for (int i = 0; i < fountainEvents.Count; i++)
        {
            var expectedHour = i; // Start at 0:30, then 1:30, 2:30, etc.
            var expectedState = (i % 2 == 0) ? "true" : "false"; // Alternate: true, false, true, false...
            
            Assert.Equal(expectedHour, fountainEvents[i].EventTime.Hour);
            Assert.Equal(30, fountainEvents[i].EventTime.Minute);
            Assert.Equal(expectedState, fountainEvents[i].Data);
        }
    }

    [Theory]
    [InlineData(0, 0, "false", "false")]  // 00:00 - lights=false (from prev day 23:00), fountain=false (from prev day 23:30)
    [InlineData(0, 30, "false", "true")]  // 00:30 - lights=false (from prev day 23:00), fountain=true (triggers now)
    [InlineData(8, 0, "true", "false")]   // 08:00 - lights=true (triggers now), fountain=false (from 07:30)
    [InlineData(8, 30, "true", "true")]   // 08:30 - lights=true (from 08:00), fountain=true (triggers now)
    [InlineData(23, 0, "false", "true")]  // 23:00 - lights=false (triggers now), fountain=true (from 22:30=true)
    [InlineData(23, 30, "false", "false")] // 23:30 - lights=false (from 23:00), fountain=false (triggers now)
    public void VerifyStateAtSpecificEventTimes(int hour, int minute, string? expectedLightsState, string expectedFountainState)
    {
        // Arrange
        _timeProvider.Now = new DateTimeOffset(2024, 6, 15, hour, minute, 0, TimeSpan.Zero);

        // Act
        var lightsSchedule = _scheduleCollection["lights"]!;
        var fountainSchedule = _scheduleCollection["fountain"]!;
        var sunTimes = _timeProvider.GetUtcSunriseAndSunset().GetAwaiter().GetResult();
        
        var lightsState = GetCurrentScheduleState(lightsSchedule, _timeProvider.Now, sunTimes);
        var fountainState = GetCurrentScheduleState(fountainSchedule, _timeProvider.Now, sunTimes);

        // Assert
        Assert.Equal(expectedLightsState, lightsState);
        Assert.Equal(expectedFountainState, fountainState);
    }
}