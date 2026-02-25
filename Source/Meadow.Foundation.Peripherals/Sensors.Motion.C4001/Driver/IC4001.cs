using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using static Meadow.Foundation.Sensors.Motion.C4001;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents a C4001 motion sensor, providing access to motion data such as
/// status, target properties, and motion detection state.
/// </summary>
public interface IC4001 : ISensor
{
    /// <summary>
    /// Sets the sensor's operating mode (e.g., normal, low-power, or test mode).
    /// </summary>
    /// <param name="mode">The <see cref="SensorMode"/> to apply to the sensor.</param>
    /// <returns><c>true</c> if the mode was successfully set; otherwise, <c>false</c>.</returns>
    bool SetSensorMode(SensorMode mode);

    /// <summary>
    /// Retrieves the current status of the sensor.
    /// </summary>
    /// <returns>The current <see cref="SensorStatus"/> indicating sensor readiness or errors.</returns>
    SensorStatus GetStatus();

    /// <summary>
    /// Retrieves the ID or number of the current detected target.
    /// </summary>
    /// <returns>A byte representing the target number.</returns>
    byte GetTargetNumber();

    /// <summary>
    /// Gets the speed of the detected target.
    /// </summary>
    /// <returns>A <see cref="Speed"/> value representing the target's speed in meters per second.</returns>
    Speed GetTargetSpeed();

    /// <summary>
    /// Gets the range (distance) to the detected target.
    /// </summary>
    /// <returns>A <see cref="Length"/> value representing the target's range in meters.</returns>
    Length GetTargetRange();

    /// <summary>
    /// Gets the energy level or strength of the detected target signal.
    /// </summary>
    /// <returns>An unsigned integer representing the target's energy level.</returns>
    uint GetTargetEnergy();

    /// <summary>
    /// Sets the detection range for the sensor.
    /// </summary>
    /// <param name="min">Minimum detection range (0.3 m – max).</param>
    /// <param name="max">Maximum detection range (2.4 m – 20.0 m).</param>
    /// <param name="trig">Trigger range (must be within min–max).</param>
    /// <returns><c>true</c> if the range was set successfully; <c>false</c> if any parameter was out of range.</returns>
    bool SetDetectionRange(Length min, Length max, Length trig);

    /// <summary>
    /// Sets the trigger sensitivity for the sensor.
    /// </summary>
    /// <returns><c>true</c> if the sensitivity was set successfully; <c>false</c> if the value was out of range (0–9).</returns>
    bool SetTrigSensitivity(byte sensitivity);

    /// <summary>
    /// Sets the keep sensitivity for the sensor, which determines the sensor's ability to maintain detection of a target once triggered.
    /// </summary>
    /// <returns><c>true</c> if the sensitivity was set successfully; <c>false</c> if the value was out of range (0–9).</returns>
    bool SetKeepSensitivity(byte sensitivity);

    /// <summary>
    /// Sets the trigger delay and keep timeout.
    /// </summary>
    /// <param name="trig">Trigger delay (0–2 s, resolution 10 ms).</param>
    /// <param name="keep">Keep timeout (2–1500 s, resolution 0.5 s).</param>
    bool SetDelay(TimeSpan trig, TimeSpan keep);

    /// <summary>
    /// Returns the current keep timeout.
    /// </summary>
    TimeSpan GetKeepTimeout();

    /// <summary>
    /// Indicates whether motion is currently detected by the sensor.
    /// </summary>
    /// <returns><c>true</c> if motion is detected; otherwise, <c>false</c>.</returns>
    bool IsMotionDetected();
}
