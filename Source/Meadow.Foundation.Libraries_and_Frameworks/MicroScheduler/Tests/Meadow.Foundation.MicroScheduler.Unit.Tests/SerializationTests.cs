namespace Meadow.Foundation.Scheduling.Tests;

/// <summary>
/// Unit tests for the ScheduleSerializer class.
/// </summary>
public class ScheduleSerializerTests
{
    #region Happy Path Tests

    [Fact]
    public void SerializeMasterSchedule_WithEmptyCollection_ReturnsValidJson()
    {
        // Arrange
        var masterSchedule = new ScheduleCollection();

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("schedules", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithSingleEmptySchedule_ReturnsValidJson()
    {
        // Arrange
        var schedule = new Schedule { Name = "Test Schedule" };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Test Schedule", json);
        Assert.Contains("schedules", json);
        Assert.Contains("events", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithDailyEvent_ReturnsValidJson()
    {
        // Arrange
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 18, 30, 0), "TURN_ON");
        var schedule = new Schedule
        {
            Name = "Living Room",
            Events = { dailyEvent }
        };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Living Room", json);
        Assert.Contains("Daily", json);
        Assert.Contains("TURN_ON", json);
        Assert.Contains("18:30:00", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithWeekdayEvent_ReturnsValidJson()
    {
        // Arrange
        var weekdayEvent = new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 7, 0, 0),
            "WAKE_UP",
            new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
        var schedule = new Schedule
        {
            Name = "Bedroom",
            Events = { weekdayEvent }
        };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Bedroom", json);
        Assert.Contains("Weekday", json);
        Assert.Contains("WAKE_UP", json);
        Assert.Contains("Monday", json);
        Assert.Contains("Wednesday", json);
        Assert.Contains("Friday", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithSunriseOffsetEvent_ReturnsValidJson()
    {
        // Arrange
        var sunriseEvent = new SunriseOffsetScheduleEvent(
            TimeSpan.FromMinutes(30),
            "SUNRISE_PLUS_30",
            new[] { DayOfWeek.Saturday, DayOfWeek.Sunday });
        var schedule = new Schedule
        {
            Name = "Garden",
            Events = { sunriseEvent }
        };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Garden", json);
        Assert.Contains("SunriseOffset", json);
        Assert.Contains("SUNRISE_PLUS_30", json);
        Assert.Contains("00:30:00", json);
        Assert.Contains("Saturday", json);
        Assert.Contains("Sunday", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithSunsetOffsetEvent_ReturnsValidJson()
    {
        // Arrange
        var sunsetEvent = new SunsetOffsetScheduleEvent(
            TimeSpan.FromMinutes(-15),
            "SUNSET_MINUS_15");
        var schedule = new Schedule
        {
            Name = "Porch",
            Events = { sunsetEvent }
        };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Porch", json);
        Assert.Contains("SunsetOffset", json);
        Assert.Contains("SUNSET_MINUS_15", json);
        Assert.Contains("-00:15:00", json);
    }

    [Fact]
    public void SerializeMasterSchedule_WithDisabledEvent_ReturnsValidJson()
    {
        // Arrange
        var dailyEvent = new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), "DISABLED_EVENT")
        {
            IsDisabled = true
        };
        var schedule = new Schedule
        {
            Name = "Test",
            Events = { dailyEvent }
        };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("true", json); // isDisabled should be true
    }

    #endregion

    #region Deserialization Tests

    [Fact]
    public void DeserializeMasterSchedule_WithValidEmptyJson_ReturnsEmptyCollection()
    {
        // Arrange
        var json = "{\"schedules\":[]}";

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Schedules);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithDailyEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Living Room",
                    "events": [
                        {
                            "eventType": "Daily",
                            "isDisabled": false,
                            "data": "TURN_ON",
                            "eventTime": "2024-01-01T18:30:00"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Schedules);

        var schedule = result.Schedules.First();
        Assert.Equal("Living Room", schedule.Name);
        Assert.Single(schedule.Events);

        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.Equal(ScheduleEventType.Daily, dailyEvent.EventType);
        Assert.Equal("TURN_ON", dailyEvent.Data);
        Assert.False(dailyEvent.IsDisabled);
        Assert.Equal(new DateTime(2024, 1, 1, 18, 30, 0), dailyEvent.EventTime);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithWeekdayEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Bedroom",
                    "events": [
                        {
                            "eventType": "Weekday",
                            "isDisabled": false,
                            "data": "WAKE_UP",
                            "eventTime": "2024-01-01T07:00:00",
                            "daysOfWeek": ["Monday", "Wednesday", "Friday"]
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        var schedule = result.Schedules.First();
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());

        Assert.Equal(ScheduleEventType.Weekday, weekdayEvent.EventType);
        Assert.Equal("WAKE_UP", weekdayEvent.Data);
        Assert.Equal(new DateTime(2024, 1, 1, 7, 0, 0), weekdayEvent.EventTime);
        Assert.Equal(3, weekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, weekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, weekdayEvent.DaysOfWeek);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithSunriseOffsetEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Garden",
                    "events": [
                        {
                            "eventType": "SunriseOffset",
                            "isDisabled": false,
                            "data": "SUNRISE_PLUS_30",
                            "offset": "00:30:00",
                            "daysOfWeek": ["Saturday", "Sunday"]
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        var schedule = result.Schedules.First();
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(schedule.Events.First());

        Assert.Equal(ScheduleEventType.SunriseOffset, sunriseEvent.EventType);
        Assert.Equal("SUNRISE_PLUS_30", sunriseEvent.Data);
        Assert.Equal(TimeSpan.FromMinutes(30), sunriseEvent.Offset);
        Assert.Equal(2, sunriseEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, sunriseEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, sunriseEvent.DaysOfWeek);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithNegativeSunsetOffset_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Porch",
                    "events": [
                        {
                            "eventType": "SunsetOffset",
                            "isDisabled": false,
                            "data": "SUNSET_MINUS_15",
                            "offset": "-00:15:00"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        var schedule = result.Schedules.First();
        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(schedule.Events.First());

        Assert.Equal(ScheduleEventType.SunsetOffset, sunsetEvent.EventType);
        Assert.Equal("SUNSET_MINUS_15", sunsetEvent.Data);
        Assert.Equal(TimeSpan.FromMinutes(-15), sunsetEvent.Offset);
        Assert.Null(sunsetEvent.DaysOfWeek); // No days specified
    }

    [Fact]
    public void DeserializeMasterSchedule_WithDisabledEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "Daily",
                            "isDisabled": true,
                            "data": "DISABLED_EVENT",
                            "eventTime": "2024-01-01T12:00:00"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        var schedule = result.Schedules.First();
        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());

        Assert.True(dailyEvent.IsDisabled);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_ComplexScheduleCollection_MaintainsAllData()
    {
        // Arrange
        var originalSchedules = new ScheduleCollection();

        var schedule1 = new Schedule { Name = "Living Room" };
        schedule1.Events.Add(new DailyScheduleEvent(new DateTime(2024, 1, 1, 18, 30, 0), "ON"));
        schedule1.Events.Add(new DailyScheduleEvent(new DateTime(2024, 1, 1, 23, 0, 0), "OFF"));

        var schedule2 = new Schedule { Name = "Garden" };
        schedule2.Events.Add(new WeekdayScheduleEvent(
            new DateTime(2024, 1, 1, 6, 0, 0),
            "WATER",
            new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }));
        schedule2.Events.Add(new SunriseOffsetScheduleEvent(TimeSpan.FromMinutes(30), "LIGHT_ON"));
        schedule2.Events.Add(new SunsetOffsetScheduleEvent(TimeSpan.FromMinutes(-15), "LIGHT_OFF", new[] { DayOfWeek.Sunday }));

        originalSchedules.Schedules.Add(schedule1);
        originalSchedules.Schedules.Add(schedule2);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalSchedules);
        var deserializedSchedules = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.Equal(originalSchedules.Schedules.Count, deserializedSchedules.Schedules.Count);

        // Verify Living Room schedule
        var livingRoom = deserializedSchedules.Schedules.First(s => s.Name == "Living Room");
        Assert.Equal(2, livingRoom.Events.Count);

        var onEvent = Assert.IsType<DailyScheduleEvent>(livingRoom.Events.First(e => e.Data == "ON"));
        Assert.Equal(new DateTime(2024, 1, 1, 18, 30, 0), onEvent.EventTime);

        var offEvent = Assert.IsType<DailyScheduleEvent>(livingRoom.Events.First(e => e.Data == "OFF"));
        Assert.Equal(new DateTime(2024, 1, 1, 23, 0, 0), offEvent.EventTime);

        // Verify Garden schedule
        var garden = deserializedSchedules.Schedules.First(s => s.Name == "Garden");
        Assert.Equal(3, garden.Events.Count);

        var waterEvent = Assert.IsType<WeekdayScheduleEvent>(garden.Events.First(e => e.Data == "WATER"));
        Assert.Equal(3, waterEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, waterEvent.DaysOfWeek);

        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(garden.Events.First(e => e.Data == "LIGHT_ON"));
        Assert.Equal(TimeSpan.FromMinutes(30), sunriseEvent.Offset);
        Assert.Null(sunriseEvent.DaysOfWeek);

        var sunsetEvent = Assert.IsType<SunsetOffsetScheduleEvent>(garden.Events.First(e => e.Data == "LIGHT_OFF"));
        Assert.Equal(TimeSpan.FromMinutes(-15), sunsetEvent.Offset);
        Assert.Single(sunsetEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, sunsetEvent.DaysOfWeek);
    }

    [Fact]
    public void RoundTrip_WithNullData_PreservesNullValues()
    {
        // Arrange
        var originalSchedule = new ScheduleCollection();
        var schedule = new Schedule { Name = "Test" };
        schedule.Events.Add(new DailyScheduleEvent(new DateTime(2024, 1, 1, 12, 0, 0), null));
        originalSchedule.Schedules.Add(schedule);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalSchedule);
        var deserializedSchedule = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        var event1 = deserializedSchedule.Schedules.First().Events.First();
        Assert.Null(event1.Data);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void DeserializeMasterSchedule_WithNullJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(null));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithEmptyJson_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(""));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(invalidJson));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithMissingSchedules_ThrowsArgumentException()
    {
        // Arrange
        var json = "{ \"notSchedules\": [] }";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(json));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidEventType_ThrowsArgumentException()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "InvalidType",
                            "isDisabled": false,
                            "data": "TEST"
                        }
                    ]
                }
            ]
        }
        """;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(json));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidDateTime_ThrowsException()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "Daily",
                            "isDisabled": false,
                            "data": "TEST",
                            "eventTime": "invalid-date"
                        }
                    ]
                }
            ]
        }
        """;

        // Act & Assert
        Assert.Throws<FormatException>(() => ScheduleSerializer.DeserializeScheduleCollection(json));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidTimeSpanOffset_ThrowsArgumentException()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "SunriseOffset",
                            "isDisabled": false,
                            "data": "TEST",
                            "offset": "invalid-timespan"
                        }
                    ]
                }
            ]
        }
        """;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(json));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithInvalidDayOfWeek_ThrowsException()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "Weekday",
                            "isDisabled": false,
                            "data": "TEST",
                            "eventTime": "2024-01-01T12:00:00",
                            "daysOfWeek": ["InvalidDay"]
                        }
                    ]
                }
            ]
        }
        """;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ScheduleSerializer.DeserializeScheduleCollection(json));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SerializeMasterSchedule_WithLargeTimeOffset_HandlesCorrectly()
    {
        // Arrange
        var largeOffset = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(45));
        var sunriseEvent = new SunriseOffsetScheduleEvent(largeOffset, "LARGE_OFFSET");
        var schedule = new Schedule { Name = "Test", Events = { sunriseEvent } };
        var masterSchedule = new ScheduleCollection(new[] { schedule });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);
        var deserialized = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        var deserializedEvent = Assert.IsType<SunriseOffsetScheduleEvent>(deserialized.Schedules.First().Events.First());
        Assert.Equal(largeOffset, deserializedEvent.Offset);
    }

    [Fact]
    public void SerializeMasterSchedule_WithMultipleSchedulesSameName_HandlesCorrectly()
    {
        // Arrange
        var schedule1 = new Schedule { Name = "Same Name" };
        schedule1.Events.Add(new DailyScheduleEvent(new DateTime(2024, 1, 1, 8, 0, 0), "FIRST"));

        var schedule2 = new Schedule { Name = "Same Name" };
        schedule2.Events.Add(new DailyScheduleEvent(new DateTime(2024, 1, 1, 20, 0, 0), "SECOND"));

        var masterSchedule = new ScheduleCollection(new[] { schedule1, schedule2 });

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(masterSchedule);
        var deserialized = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.Equal(2, deserialized.Schedules.Count);
        Assert.All(deserialized.Schedules, s => Assert.Equal("Same Name", s.Name));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithEmptyOffset_ParsesAsZero()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "SunriseOffset",
                            "isDisabled": false,
                            "data": "ZERO_OFFSET",
                            "offset": ""
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(result.Schedules.First().Events.First());
        Assert.Equal(TimeSpan.Zero, sunriseEvent.Offset);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithNullDaysOfWeek_HandlesCorrectly()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "circuitName": "Test",
                    "events": [
                        {
                            "eventType": "SunriseOffset",
                            "isDisabled": false,
                            "data": "NO_DAYS",
                            "offset": "00:30:00",
                            "daysOfWeek": null
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(result.Schedules.First().Events.First());
        Assert.Null(sunriseEvent.DaysOfWeek);
    }

    #endregion

    #region Integration Tests with Real Data

    [Fact]
    public void DeserializeFromFile_TestSchedule1_ParsesCorrectly()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var json = File.ReadAllText(filePath);

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Schedules.Count);

        // Verify Light Schedule
        var lightSchedule = result.Schedules.First(s => s.Name == "light");
        Assert.Equal("light", lightSchedule.Name);
        Assert.Equal(5, lightSchedule.Events.Count);

        // Verify Light Daily Events
        var lightDailyEvents = lightSchedule.Events.OfType<DailyScheduleEvent>().ToList();
        Assert.Equal(2, lightDailyEvents.Count);

        var lightOnEvent = lightDailyEvents.First(e => e.Data == "true");
        Assert.Equal(new DateTime(2024, 1, 1, 18, 0, 0), lightOnEvent.EventTime);
        Assert.False(lightOnEvent.IsDisabled);

        var lightOffEvent = lightDailyEvents.First(e => e.Data == "false");
        Assert.Equal(new DateTime(2024, 1, 1, 23, 30, 0), lightOffEvent.EventTime);
        Assert.False(lightOffEvent.IsDisabled);

        // Verify Light Weekday Event
        var lightWeekdayEvent = lightSchedule.Events.OfType<WeekdayScheduleEvent>().Single();
        Assert.Equal("true", lightWeekdayEvent.Data);
        Assert.Equal(new DateTime(2024, 1, 1, 17, 30, 0), lightWeekdayEvent.EventTime);
        Assert.False(lightWeekdayEvent.IsDisabled);
        Assert.Equal(5, lightWeekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, lightWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Tuesday, lightWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, lightWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Thursday, lightWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, lightWeekdayEvent.DaysOfWeek);

        // Verify Light Sunset Offset Event
        var lightSunsetEvent = lightSchedule.Events.OfType<SunsetOffsetScheduleEvent>().Single();
        Assert.Equal("true", lightSunsetEvent.Data);
        Assert.Equal(TimeSpan.FromMinutes(-30), lightSunsetEvent.Offset);
        Assert.False(lightSunsetEvent.IsDisabled);
        Assert.Equal(2, lightSunsetEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, lightSunsetEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, lightSunsetEvent.DaysOfWeek);

        // Verify Light Sunrise Offset Event (Disabled)
        var lightSunriseEvent = lightSchedule.Events.OfType<SunriseOffsetScheduleEvent>().Single();
        Assert.Equal("false", lightSunriseEvent.Data);
        Assert.Equal(TimeSpan.FromHours(1), lightSunriseEvent.Offset);
        Assert.True(lightSunriseEvent.IsDisabled); // This one is disabled
        Assert.Null(lightSunriseEvent.DaysOfWeek); // Should be null

        // Verify Fountain Schedule
        var fountainSchedule = result.Schedules.First(s => s.Name == "fountain");
        Assert.Equal("fountain", fountainSchedule.Name);
        Assert.Equal(5, fountainSchedule.Events.Count);

        // Verify Fountain Daily Events
        var fountainDailyEvents = fountainSchedule.Events.OfType<DailyScheduleEvent>().ToList();
        Assert.Equal(2, fountainDailyEvents.Count);

        var fountainOnEvent = fountainDailyEvents.First(e => e.Data == "true");
        Assert.Equal(new DateTime(2024, 1, 1, 8, 0, 0), fountainOnEvent.EventTime);
        Assert.False(fountainOnEvent.IsDisabled);

        var fountainOffEvent = fountainDailyEvents.First(e => e.Data == "false");
        Assert.Equal(new DateTime(2024, 1, 1, 22, 0, 0), fountainOffEvent.EventTime);
        Assert.False(fountainOffEvent.IsDisabled);

        // Verify Fountain Weekday Event
        var fountainWeekdayEvent = fountainSchedule.Events.OfType<WeekdayScheduleEvent>().Single();
        Assert.Equal("true", fountainWeekdayEvent.Data);
        Assert.Equal(new DateTime(2024, 1, 1, 6, 30, 0), fountainWeekdayEvent.EventTime);
        Assert.False(fountainWeekdayEvent.IsDisabled);
        Assert.Equal(3, fountainWeekdayEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Monday, fountainWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Wednesday, fountainWeekdayEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Friday, fountainWeekdayEvent.DaysOfWeek);

        // Verify Fountain Sunrise Offset Event
        var fountainSunriseEvent = fountainSchedule.Events.OfType<SunriseOffsetScheduleEvent>().Single();
        Assert.Equal("true", fountainSunriseEvent.Data);
        Assert.Equal(TimeSpan.FromMinutes(15), fountainSunriseEvent.Offset);
        Assert.False(fountainSunriseEvent.IsDisabled);
        Assert.Equal(2, fountainSunriseEvent.DaysOfWeek.Length);
        Assert.Contains(DayOfWeek.Saturday, fountainSunriseEvent.DaysOfWeek);
        Assert.Contains(DayOfWeek.Sunday, fountainSunriseEvent.DaysOfWeek);

        // Verify Fountain Sunset Offset Event
        var fountainSunsetEvent = fountainSchedule.Events.OfType<SunsetOffsetScheduleEvent>().Single();
        Assert.Equal("false", fountainSunsetEvent.Data);
        Assert.Equal(TimeSpan.FromHours(2), fountainSunsetEvent.Offset);
        Assert.False(fountainSunsetEvent.IsDisabled);
        Assert.Null(fountainSunsetEvent.DaysOfWeek); // Should be null
    }

    [Fact]
    public void RoundTrip_TestSchedule1_MaintainsAllData()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        Assert.True(File.Exists(filePath), $"Test file not found: {filePath}");

        var originalJson = File.ReadAllText(filePath);

        // Act - Deserialize then re-serialize
        var scheduleCollection = ScheduleSerializer.DeserializeScheduleCollection(originalJson);
        var newJson = ScheduleSerializer.SerializeScheduleCollection(scheduleCollection);
        var roundTripCollection = ScheduleSerializer.DeserializeScheduleCollection(newJson);

        // Assert - Verify the data is identical after round trip
        Assert.Equal(scheduleCollection.Schedules.Count, roundTripCollection.Schedules.Count);

        for (int i = 0; i < scheduleCollection.Schedules.Count; i++)
        {
            var originalSchedule = scheduleCollection.Schedules[i];
            var roundTripSchedule = roundTripCollection.Schedules[i];

            Assert.Equal(originalSchedule.Name, roundTripSchedule.Name);
            Assert.Equal(originalSchedule.Events.Count, roundTripSchedule.Events.Count);

            // Compare each event
            for (int j = 0; j < originalSchedule.Events.Count; j++)
            {
                var originalEvent = originalSchedule.Events[j];
                var roundTripEvent = roundTripSchedule.Events[j];

                Assert.Equal(originalEvent.EventType, roundTripEvent.EventType);
                Assert.Equal(originalEvent.IsDisabled, roundTripEvent.IsDisabled);
                Assert.Equal(originalEvent.Data, roundTripEvent.Data);

                // Type-specific comparisons
                switch (originalEvent)
                {
                    case DailyScheduleEvent originalDaily:
                        var roundTripDaily = Assert.IsType<DailyScheduleEvent>(roundTripEvent);
                        Assert.Equal(originalDaily.EventTime, roundTripDaily.EventTime);
                        break;

                    case WeekdayScheduleEvent originalWeekday:
                        var roundTripWeekday = Assert.IsType<WeekdayScheduleEvent>(roundTripEvent);
                        Assert.Equal(originalWeekday.EventTime, roundTripWeekday.EventTime);
                        Assert.Equal(originalWeekday.DaysOfWeek.Length, roundTripWeekday.DaysOfWeek.Length);
                        Assert.True(originalWeekday.DaysOfWeek.SequenceEqual(roundTripWeekday.DaysOfWeek));
                        break;

                    case SunriseOffsetScheduleEvent originalSunrise:
                        var roundTripSunrise = Assert.IsType<SunriseOffsetScheduleEvent>(roundTripEvent);
                        Assert.Equal(originalSunrise.Offset, roundTripSunrise.Offset);
                        if (originalSunrise.DaysOfWeek == null)
                            Assert.Null(roundTripSunrise.DaysOfWeek);
                        else
                            Assert.True(originalSunrise.DaysOfWeek.SequenceEqual(roundTripSunrise.DaysOfWeek));
                        break;

                    case SunsetOffsetScheduleEvent originalSunset:
                        var roundTripSunset = Assert.IsType<SunsetOffsetScheduleEvent>(roundTripEvent);
                        Assert.Equal(originalSunset.Offset, roundTripSunset.Offset);
                        if (originalSunset.DaysOfWeek == null)
                            Assert.Null(roundTripSunset.DaysOfWeek);
                        else
                            Assert.True(originalSunset.DaysOfWeek.SequenceEqual(roundTripSunset.DaysOfWeek));
                        break;
                }
            }
        }
    }

    [Fact]
    public void TestSchedule1_VerifyScheduleProperties()
    {
        // Arrange
        var filePath = Path.Combine("inputs", "test_schedule_1.json");
        var json = File.ReadAllText(filePath);

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert - Test the computed properties
        var lightSchedule = result.Schedules.First(s => s.Name == "light");
        var fountainSchedule = result.Schedules.First(s => s.Name == "fountain");

        // Both schedules should have sunrise/sunset events
        Assert.True(lightSchedule.ContainsSunriseOrSunsetEvents);
        Assert.True(fountainSchedule.ContainsSunriseOrSunsetEvents);

        // The master schedule should also indicate it contains sunrise/sunset events
        Assert.True(result.ContainsSunriseOrSunsetEvents);

        // Verify event counts by type
        Assert.Equal(2, lightSchedule.Events.Count(e => e.EventType == ScheduleEventType.Daily));
        Assert.Equal(1, lightSchedule.Events.Count(e => e.EventType == ScheduleEventType.Weekday));
        Assert.Equal(1, lightSchedule.Events.Count(e => e.EventType == ScheduleEventType.SunriseOffset));
        Assert.Equal(1, lightSchedule.Events.Count(e => e.EventType == ScheduleEventType.SunsetOffset));

        Assert.Equal(2, fountainSchedule.Events.Count(e => e.EventType == ScheduleEventType.Daily));
        Assert.Equal(1, fountainSchedule.Events.Count(e => e.EventType == ScheduleEventType.Weekday));
        Assert.Equal(1, fountainSchedule.Events.Count(e => e.EventType == ScheduleEventType.SunriseOffset));
        Assert.Equal(1, fountainSchedule.Events.Count(e => e.EventType == ScheduleEventType.SunsetOffset));

        // Verify disabled event count
        Assert.Equal(1, lightSchedule.Events.Count(e => e.IsDisabled));
        Assert.Equal(0, fountainSchedule.Events.Count(e => e.IsDisabled));
    }

    #endregion
}