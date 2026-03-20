using Meadow.Foundation.RTCs;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation;

/// <summary>
/// Represents a Wilderness Labs DeepSleep power controller
/// </summary>
public class DeepSleep
{
    /// <summary>
    /// Has the timer ended
    /// </summary>
    public bool HasTimerEnded => rtc.HasTimerEnded;

    /// <summary>
    /// Has the alarm triggered
    /// </summary>
    public bool HasAlarmTriggered => rtc.HasAlarmTriggered;

    private Ab0805 rtc;

    /// <summary>
    /// Creates a new DeepSleep object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public DeepSleep(II2cBus i2cBus)
    {
        rtc = new Ab0805(i2cBus);
    }

    /// <summary>
    /// Start DeepSleep
    /// </summary>
    /// <param name="delayInSeconds">Delay in seconds before powering off</param>
    /// <param name="wakeTime">Time to wake device</param>
    public void SetDeepSleep(byte delayInSeconds, DateTimeOffset wakeTime)
    {
        rtc.ResetTimer();
        rtc.ResetAlarm();

        rtc.StartTimer(delayInSeconds, Ab0805.DelayTimeUnit.Seconds);
        rtc.SetAlarm(wakeTime);
    }

    /// <summary>
    /// Gets the current time from the RTC.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/> object representing the current time.</returns>
    public DateTimeOffset GetTime() => rtc.GetTime();

    /// <summary>
    /// Sets the time on the RTC.
    /// </summary>
    /// <param name="time">The <see cref="DateTimeOffset"/> to set.</param>
    public void SetTime(DateTimeOffset time) => rtc.SetTime(time);

    /// <summary>
    /// Programs the underlying RTC to assert its alarm interrupt at the specified date and time, enabling deep sleep wake-up.
    /// </summary>
    /// <param name="alarmTime">The DateTimeOffset to trigger the alarm</param>
    public void SetAlarm(DateTimeOffset alarmTime) => rtc.SetAlarm(alarmTime);

    /// <summary>
    /// Start the timer on the RTC
    /// </summary>
    /// <param name="timerValue">Count down timer value as an integer in seconds</param>
    public void StartTimer(byte timerValue) => rtc.StartTimer(timerValue);

    /// <summary>
    /// Reset the timer on the RTC.
    /// </summary>
    public void ResetTimer() => rtc.ResetTimer();

    /// <summary>
    /// Reset the alarm on the RTC.
    /// </summary>
    public void ResetAlarm() => rtc.ResetAlarm();
}