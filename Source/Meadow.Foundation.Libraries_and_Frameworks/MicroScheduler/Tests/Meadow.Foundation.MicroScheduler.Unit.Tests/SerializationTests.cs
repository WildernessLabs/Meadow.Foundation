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
        var dailyEvent = new DailyScheduleEvent(new DateTimeOffset(2024, 1, 1, 18, 30, 0, TimeSpan.Zero), "TURN_ON");
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
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithDailyEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "name": "Living Room",
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
        Assert.Equal(1, result.Count);

        var schedule = result[0];
        Assert.Equal("Living Room", schedule.Name);
        Assert.Single(schedule.Events);

        var dailyEvent = Assert.IsType<DailyScheduleEvent>(schedule.Events.First());
        Assert.Equal(ScheduleEventType.Daily, dailyEvent.EventType);
        Assert.Equal("TURN_ON", dailyEvent.Data);
        Assert.False(dailyEvent.IsDisabled);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 18, 30, 0, TimeSpan.Zero), dailyEvent.EventTime);
    }

    [Fact]
    public void DeserializeMasterSchedule_WithWeekdayEvent_ReturnsCorrectSchedule()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "name": "Bedroom",
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
        var schedule = result[0];
        var weekdayEvent = Assert.IsType<WeekdayScheduleEvent>(schedule.Events.First());

        Assert.Equal(ScheduleEventType.Weekday, weekdayEvent.EventType);
        Assert.Equal("WAKE_UP", weekdayEvent.Data);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.Zero), weekdayEvent.EventTime);
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
                    "name": "Garden",
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
        var schedule = result[0];
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
                    "name": "Porch",
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
        var schedule = result[0];
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
                    "name": "Test",
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
        var schedule = result[0];
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

        originalSchedules.Add(schedule1);
        originalSchedules.Add(schedule2);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalSchedules);
        var deserializedSchedules = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.Equal(originalSchedules.Count, deserializedSchedules.Count);

        // Verify Living Room schedule
        var livingRoom = deserializedSchedules["Living Room"]!;
        Assert.Equal(2, livingRoom.Events.Count);

        var onEvent = Assert.IsType<DailyScheduleEvent>(livingRoom.Events.First(e => e.Data == "ON"));
        Assert.Equal(new DateTime(2024, 1, 1, 18, 30, 0), onEvent.EventTime);

        var offEvent = Assert.IsType<DailyScheduleEvent>(livingRoom.Events.First(e => e.Data == "OFF"));
        Assert.Equal(new DateTime(2024, 1, 1, 23, 0, 0), offEvent.EventTime);

        // Verify Garden schedule
        var garden = deserializedSchedules["Garden"]!;
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
        originalSchedule.Add(schedule);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalSchedule);
        var deserializedSchedule = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        var event1 = deserializedSchedule[0].Events.First();
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
                    "name": "Test",
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
                    "name": "Test",
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
                    "name": "Test",
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
                    "name": "Test",
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
        var deserializedEvent = Assert.IsType<SunriseOffsetScheduleEvent>(deserialized[0].Events.First());
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
        Assert.Equal(2, deserialized.Count);
        Assert.All(deserialized, s => Assert.Equal("Same Name", s.Name));
    }

    [Fact]
    public void DeserializeMasterSchedule_WithEmptyOffset_ParsesAsZero()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "name": "Test",
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
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(result[0].Events.First());
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
                    "name": "Test",
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
        var sunriseEvent = Assert.IsType<SunriseOffsetScheduleEvent>(result[0].Events.First());
        Assert.Null(sunriseEvent.DaysOfWeek);
    }

    #endregion

    #region Timezone Serialization Tests

    [Fact]
    public void SerializeScheduleCollection_WithTimezone_IncludesTimezoneInfo()
    {
        // Arrange
        var schedule = new Schedule { Name = "Test Schedule" };
        var collection = new ScheduleCollection(new[] { schedule })
        {
            Timezone = new TimezoneInfo
            {
                TimezoneName = "America/Chicago",
                UtcOffsetHours = -6.0,
                DaylightSavingTime = new DaylightSavingTimeInfo
                {
                    StartMonth = 3,
                    StartDay = 0,
                    StartDayOfWeek = DayOfWeek.Sunday,
                    StartHour = 2,
                    EndMonth = 11,
                    EndDay = 0,
                    EndDayOfWeek = DayOfWeek.Sunday,
                    EndHour = 2,
                    OffsetHours = 1.0
                }
            }
        };

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(collection);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"timezone\"", json);
        Assert.Contains("\"timezoneName\"", json);
        Assert.Contains("\"America/Chicago\"", json);
        Assert.Contains("\"utcOffsetHours\"", json);
        Assert.Contains("-6", json);
        Assert.Contains("\"daylightSavingTime\"", json);
        Assert.Contains("\"startMonth\"", json);
        Assert.Contains("\"endMonth\"", json);
        Assert.Contains("\"offsetHours\"", json);
    }

    [Fact]
    public void SerializeScheduleCollection_WithTimezoneNoDST_SerializesCorrectly()
    {
        // Arrange
        var schedule = new Schedule { Name = "Test Schedule" };
        var collection = new ScheduleCollection(new[] { schedule })
        {
            Timezone = new TimezoneInfo
            {
                TimezoneName = "UTC",
                UtcOffsetHours = 0.0,
                DaylightSavingTime = null // No DST
            }
        };

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(collection);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"timezone\"", json);
        Assert.Contains("\"timezoneName\"", json);
        Assert.Contains("\"UTC\"", json);
        Assert.Contains("\"utcOffsetHours\"", json);
        Assert.Contains("0", json);
        // Should not contain DST info when null
        Assert.DoesNotContain("\"daylightSavingTime\"", json);
    }

    [Fact]
    public void DeserializeScheduleCollection_WithTimezone_ParsesTimezoneCorrectly()
    {
        // Arrange
        var json = """
        {
            "timezone": {
                "timezoneName": "America/New_York",
                "utcOffsetHours": -5.0,
                "daylightSavingTime": {
                    "startMonth": 3,
                    "startDay": 0,
                    "startDayOfWeek": "Sunday",
                    "startHour": 2,
                    "endMonth": 11,
                    "endDay": 0,
                    "endDayOfWeek": "Sunday",
                    "endHour": 2,
                    "offsetHours": 1.0
                }
            },
            "schedules": [
                {
                    "name": "Test Schedule",
                    "events": []
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Timezone);
        Assert.Equal("America/New_York", result.Timezone.TimezoneName);
        Assert.Equal(-5.0, result.Timezone.UtcOffsetHours);
        Assert.NotNull(result.Timezone.DaylightSavingTime);
        Assert.Equal(3, result.Timezone.DaylightSavingTime.StartMonth);
        Assert.Equal(0, result.Timezone.DaylightSavingTime.StartDay);
        Assert.Equal(DayOfWeek.Sunday, result.Timezone.DaylightSavingTime.StartDayOfWeek);
        Assert.Equal(2, result.Timezone.DaylightSavingTime.StartHour);
        Assert.Equal(11, result.Timezone.DaylightSavingTime.EndMonth);
        Assert.Equal(0, result.Timezone.DaylightSavingTime.EndDay);
        Assert.Equal(DayOfWeek.Sunday, result.Timezone.DaylightSavingTime.EndDayOfWeek);
        Assert.Equal(2, result.Timezone.DaylightSavingTime.EndHour);
        Assert.Equal(1.0, result.Timezone.DaylightSavingTime.OffsetHours);
    }

    [Fact]
    public void DeserializeScheduleCollection_WithTimezoneNoDST_ParsesCorrectly()
    {
        // Arrange
        var json = """
        {
            "timezone": {
                "timezoneName": "UTC",
                "utcOffsetHours": 0.0
            },
            "schedules": [
                {
                    "name": "Test Schedule",
                    "events": []
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Timezone);
        Assert.Equal("UTC", result.Timezone.TimezoneName);
        Assert.Equal(0.0, result.Timezone.UtcOffsetHours);
        Assert.Null(result.Timezone.DaylightSavingTime);
    }

    [Fact]
    public void DeserializeScheduleCollection_WithoutTimezone_UsesDefaultTimezone()
    {
        // Arrange
        var json = """
        {
            "schedules": [
                {
                    "name": "Test Schedule",
                    "events": []
                }
            ]
        }
        """;

        // Act
        var result = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Timezone);
        Assert.Equal("UTC", result.Timezone.TimezoneName);
        Assert.Equal(0.0, result.Timezone.UtcOffsetHours);
        Assert.Null(result.Timezone.DaylightSavingTime);
    }

    [Fact]
    public void RoundTrip_TimezoneWithDST_MaintainsAllData()
    {
        // Arrange
        var originalCollection = new ScheduleCollection();
        originalCollection.Timezone = new TimezoneInfo
        {
            TimezoneName = "America/Denver",
            UtcOffsetHours = -7.0,
            DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = 3,
                StartDay = 0,
                StartDayOfWeek = DayOfWeek.Sunday,
                StartHour = 2,
                EndMonth = 11,
                EndDay = 0,
                EndDayOfWeek = DayOfWeek.Sunday,
                EndHour = 2,
                OffsetHours = 1.0
            }
        };

        var schedule = new Schedule { Name = "Mountain Schedule" };
        originalCollection.Add(schedule);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalCollection);
        var deserializedCollection = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.Equal(originalCollection.Timezone.TimezoneName, deserializedCollection.Timezone.TimezoneName);
        Assert.Equal(originalCollection.Timezone.UtcOffsetHours, deserializedCollection.Timezone.UtcOffsetHours);

        Assert.NotNull(deserializedCollection.Timezone.DaylightSavingTime);
        var originalDST = originalCollection.Timezone.DaylightSavingTime!;
        var deserializedDST = deserializedCollection.Timezone.DaylightSavingTime!;

        Assert.Equal(originalDST.StartMonth, deserializedDST.StartMonth);
        Assert.Equal(originalDST.StartDay, deserializedDST.StartDay);
        Assert.Equal(originalDST.StartDayOfWeek, deserializedDST.StartDayOfWeek);
        Assert.Equal(originalDST.StartHour, deserializedDST.StartHour);
        Assert.Equal(originalDST.EndMonth, deserializedDST.EndMonth);
        Assert.Equal(originalDST.EndDay, deserializedDST.EndDay);
        Assert.Equal(originalDST.EndDayOfWeek, deserializedDST.EndDayOfWeek);
        Assert.Equal(originalDST.EndHour, deserializedDST.EndHour);
        Assert.Equal(originalDST.OffsetHours, deserializedDST.OffsetHours);
    }

    [Fact]
    public void RoundTrip_TimezoneWithoutDST_MaintainsAllData()
    {
        // Arrange
        var originalCollection = new ScheduleCollection();
        originalCollection.Timezone = new TimezoneInfo
        {
            TimezoneName = "Asia/Tokyo",
            UtcOffsetHours = 9.0,
            DaylightSavingTime = null
        };

        var schedule = new Schedule { Name = "Tokyo Schedule" };
        originalCollection.Add(schedule);

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(originalCollection);
        var deserializedCollection = ScheduleSerializer.DeserializeScheduleCollection(json);

        // Assert
        Assert.Equal(originalCollection.Timezone.TimezoneName, deserializedCollection.Timezone.TimezoneName);
        Assert.Equal(originalCollection.Timezone.UtcOffsetHours, deserializedCollection.Timezone.UtcOffsetHours);
        Assert.Null(deserializedCollection.Timezone.DaylightSavingTime);
    }

    #endregion

    #region Timezone DST Functionality Tests

    [Fact]
    public void TimezoneInfo_DST_CalculatesCorrectly()
    {
        // Arrange - Test timezone with DST
        var timezone = new TimezoneInfo
        {
            TimezoneName = "America/Chicago",
            UtcOffsetHours = -6.0,
            DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = 3,     // March
                StartDay = 0,       // Last occurrence  
                StartDayOfWeek = DayOfWeek.Sunday,
                StartHour = 2,      // 2 AM
                EndMonth = 11,      // November
                EndDay = 0,         // Last occurrence
                EndDayOfWeek = DayOfWeek.Sunday,
                EndHour = 2,        // 2 AM
                OffsetHours = 1.0   // +1 hour during DST
            }
        };

        // Test summer time (July - should be DST active)
        var summerDate = new DateTime(2024, 7, 15, 12, 0, 0, DateTimeKind.Utc);
        var isDSTActiveSummer = timezone.IsDaylightSavingTimeActive(summerDate);
        var totalOffsetSummer = timezone.GetTotalUtcOffset(summerDate);

        // Test winter time (January - should not be DST active)
        var winterDate = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var isDSTActiveWinter = timezone.IsDaylightSavingTimeActive(winterDate);
        var totalOffsetWinter = timezone.GetTotalUtcOffset(winterDate);

        // Assert
        Assert.True(isDSTActiveSummer, "DST should be active in July");
        Assert.Equal(-5.0, totalOffsetSummer); // -6 + 1 = -5

        Assert.False(isDSTActiveWinter, "DST should not be active in January");
        Assert.Equal(-6.0, totalOffsetWinter); // -6 + 0 = -6
    }

    [Fact]
    public void TimezoneInfo_WithoutDST_CalculatesCorrectly()
    {
        // Arrange - Test timezone without DST
        var timezone = new TimezoneInfo
        {
            TimezoneName = "Asia/Tokyo",
            UtcOffsetHours = 9.0,
            DaylightSavingTime = null // No DST
        };

        // Test any date
        var testDate = new DateTime(2024, 7, 15, 12, 0, 0, DateTimeKind.Utc);
        var isDSTActive = timezone.IsDaylightSavingTimeActive(testDate);
        var totalOffset = timezone.GetTotalUtcOffset(testDate);

        // Assert
        Assert.False(isDSTActive, "DST should never be active when not configured");
        Assert.Equal(9.0, totalOffset); // Always +9
    }

    [Fact]
    public void TimezoneInfo_ConvertUtcToLocal_WorksCorrectly()
    {
        // Arrange
        var timezone = new TimezoneInfo
        {
            TimezoneName = "America/New_York",
            UtcOffsetHours = -5.0,
            DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = 3,
                StartDay = 0,
                StartDayOfWeek = DayOfWeek.Sunday,
                StartHour = 2,
                EndMonth = 11,
                EndDay = 0,
                EndDayOfWeek = DayOfWeek.Sunday,
                EndHour = 2,
                OffsetHours = 1.0
            }
        };

        // Test summer conversion (DST active)
        var utcSummer = new DateTimeOffset(2024, 7, 15, 16, 30, 0, TimeSpan.Zero); // 4:30 PM UTC
        var localSummer = timezone.ConvertUtcToLocal(utcSummer);

        // Test winter conversion (DST not active)
        var utcWinter = new DateTimeOffset(2024, 1, 15, 16, 30, 0, TimeSpan.Zero); // 4:30 PM UTC
        var localWinter = timezone.ConvertUtcToLocal(utcWinter);

        // Assert
        // Summer: UTC 16:30 -> EDT 12:30 (UTC-4 during DST)
        Assert.Equal(new DateTimeOffset(2024, 7, 15, 12, 30, 0, TimeSpan.Zero), localSummer);

        // Winter: UTC 16:30 -> EST 11:30 (UTC-5 during standard time)
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 11, 30, 0, TimeSpan.Zero), localWinter);
    }

    [Fact]
    public void TimezoneInfo_ConvertLocalToUtc_WorksCorrectly()
    {
        // Arrange
        var timezone = new TimezoneInfo
        {
            TimezoneName = "America/Los_Angeles",
            UtcOffsetHours = -8.0,
            DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = 3,
                StartDay = 0,
                StartDayOfWeek = DayOfWeek.Sunday,
                StartHour = 2,
                EndMonth = 11,
                EndDay = 0,
                EndDayOfWeek = DayOfWeek.Sunday,
                EndHour = 2,
                OffsetHours = 1.0
            }
        };

        // Test summer conversion (DST active)
        var localSummer = new DateTime(2024, 7, 15, 9, 0, 0); // 9:00 AM PDT
        var utcSummer = timezone.ConvertLocalToUtc(localSummer);

        // Test winter conversion (DST not active)
        var localWinter = new DateTime(2024, 1, 15, 9, 0, 0); // 9:00 AM PST
        var utcWinter = timezone.ConvertLocalToUtc(localWinter);

        // Assert
        // Summer: PDT 9:00 -> UTC 16:00 (PDT is UTC-7)
        Assert.Equal(new DateTime(2024, 7, 15, 16, 0, 0, DateTimeKind.Utc), utcSummer);

        // Winter: PST 9:00 -> UTC 17:00 (PST is UTC-8)
        Assert.Equal(new DateTime(2024, 1, 15, 17, 0, 0, DateTimeKind.Utc), utcWinter);
    }

    [Fact]
    public void SerializeScheduleCollection_PreservesTimezoneInJson()
    {
        // Arrange
        var collection = new ScheduleCollection();
        collection.Timezone = new TimezoneInfo
        {
            TimezoneName = "America/Chicago",
            UtcOffsetHours = -6.0,
            DaylightSavingTime = new DaylightSavingTimeInfo
            {
                StartMonth = 3,
                StartDay = 0,
                StartDayOfWeek = DayOfWeek.Sunday,
                StartHour = 2,
                EndMonth = 11,
                EndDay = 0,
                EndDayOfWeek = DayOfWeek.Sunday,
                EndHour = 2,
                OffsetHours = 1.0
            }
        };

        // Act
        var json = ScheduleSerializer.SerializeScheduleCollection(collection);

        // Assert - Verify the JSON structure contains timezone information
        Assert.Contains("\"timezone\"", json);
        Assert.Contains("\"timezoneName\": \"America/Chicago\"", json);
        Assert.Contains("\"utcOffsetHours\": -6", json);
        Assert.Contains("\"daylightSavingTime\"", json);
        Assert.Contains("\"startMonth\": 3", json);
        Assert.Contains("\"endMonth\": 11", json);
        Assert.Contains("\"offsetHours\": 1", json);
        Assert.Contains("\"startDayOfWeek\": \"Sunday\"", json);
        Assert.Contains("\"endDayOfWeek\": \"Sunday\"", json);
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
        Assert.Equal(2, result.Count);

        // Verify Timezone Information
        Assert.NotNull(result.Timezone);
        Assert.Equal("America/Chicago", result.Timezone.TimezoneName);
        Assert.Equal(-6.0, result.Timezone.UtcOffsetHours);
        Assert.NotNull(result.Timezone.DaylightSavingTime);
        Assert.Equal(3, result.Timezone.DaylightSavingTime.StartMonth);
        Assert.Equal(11, result.Timezone.DaylightSavingTime.EndMonth);
        Assert.Equal(1.0, result.Timezone.DaylightSavingTime.OffsetHours);

        // Verify Light Schedule
        var lightSchedule = result["light"]!;
        Assert.Equal("light", lightSchedule.Name);
        Assert.Equal(5, lightSchedule.Events.Count);

        // Verify Light Daily Events
        var lightDailyEvents = lightSchedule.Events.OfType<DailyScheduleEvent>().ToList();
        Assert.Equal(2, lightDailyEvents.Count);

        var lightOnEvent = lightDailyEvents.First(e => e.Data == "true");
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 18, 0, 0, TimeSpan.Zero), lightOnEvent.EventTime);
        Assert.False(lightOnEvent.IsDisabled);

        var lightOffEvent = lightDailyEvents.First(e => e.Data == "false");
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 23, 30, 0, TimeSpan.Zero), lightOffEvent.EventTime);
        Assert.False(lightOffEvent.IsDisabled);

        // Verify Light Weekday Event
        var lightWeekdayEvent = lightSchedule.Events.OfType<WeekdayScheduleEvent>().Single();
        Assert.Equal("true", lightWeekdayEvent.Data);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 17, 30, 0, TimeSpan.Zero), lightWeekdayEvent.EventTime);
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
        var fountainSchedule = result["fountain"]!;
        Assert.Equal("fountain", fountainSchedule.Name);
        Assert.Equal(5, fountainSchedule.Events.Count);

        // Verify Fountain Daily Events
        var fountainDailyEvents = fountainSchedule.Events.OfType<DailyScheduleEvent>().ToList();
        Assert.Equal(2, fountainDailyEvents.Count);

        var fountainOnEvent = fountainDailyEvents.First(e => e.Data == "true");
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero), fountainOnEvent.EventTime);
        Assert.False(fountainOnEvent.IsDisabled);

        var fountainOffEvent = fountainDailyEvents.First(e => e.Data == "false");
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero), fountainOffEvent.EventTime);
        Assert.False(fountainOffEvent.IsDisabled);

        // Verify Fountain Weekday Event
        var fountainWeekdayEvent = fountainSchedule.Events.OfType<WeekdayScheduleEvent>().Single();
        Assert.Equal("true", fountainWeekdayEvent.Data);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 6, 30, 0, TimeSpan.Zero), fountainWeekdayEvent.EventTime);
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
        Assert.Equal(scheduleCollection.Count, roundTripCollection.Count);

        // Verify timezone data is preserved
        Assert.Equal(scheduleCollection.Timezone.TimezoneName, roundTripCollection.Timezone.TimezoneName);
        Assert.Equal(scheduleCollection.Timezone.UtcOffsetHours, roundTripCollection.Timezone.UtcOffsetHours);
        if (scheduleCollection.Timezone.DaylightSavingTime != null)
        {
            Assert.NotNull(roundTripCollection.Timezone.DaylightSavingTime);
            var originalDST = scheduleCollection.Timezone.DaylightSavingTime;
            var roundTripDST = roundTripCollection.Timezone.DaylightSavingTime;
            Assert.Equal(originalDST.StartMonth, roundTripDST.StartMonth);
            Assert.Equal(originalDST.EndMonth, roundTripDST.EndMonth);
            Assert.Equal(originalDST.OffsetHours, roundTripDST.OffsetHours);
        }
        else
        {
            Assert.Null(roundTripCollection.Timezone.DaylightSavingTime);
        }

        for (int i = 0; i < scheduleCollection.Count; i++)
        {
            var originalSchedule = scheduleCollection[i];
            var roundTripSchedule = roundTripCollection[i];

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
        var lightSchedule = result["light"]!;
        var fountainSchedule = result["fountain"]!;

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