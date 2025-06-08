namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Specifies the type of schedule event.
/// </summary>
public enum ScheduleEventType
{
    /// <summary>
    /// An event that occurs daily at a specific time.
    /// </summary>
    Daily,

    /// <summary>
    /// An event that occurs on specific days of the week at a specific time.
    /// </summary>
    Weekday,

    /// <summary>
    /// An event that occurs at a specific offset from sunrise.
    /// </summary>
    SunriseOffset,

    /// <summary>
    /// An event that occurs at a specific offset from sunset.
    /// </summary>
    SunsetOffset

    // TODO: once, weekly, monthly?
}