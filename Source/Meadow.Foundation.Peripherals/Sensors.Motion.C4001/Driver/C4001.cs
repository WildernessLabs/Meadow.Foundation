using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Create a new C4001 object
/// </summary>
public partial class C4001 : IC4001
{
    /// <summary>
    /// The type of communication used by the sensor (I2C or Serial).
    /// </summary>
    private readonly CommunicationType communication;

    /// <summary>
    /// Buffer to hold private sensor data.
    /// </summary>
    private SensorMotionData motionData = new();

    /// <summary>
    /// The debounce counter.
    /// </summary>
    private int motionTimeoutCount = 0;

    /// <summary>
    /// Set the Sensor sampling mode
    /// </summary>
    public bool SetSensorMode(SensorMode mode)
    {
        if (communication == CommunicationType.I2C)
            return SetSensorModeI2c(mode);
        SetSensorModeSerial(mode);
        return true;
    }

    /// <summary>
    /// Get the current status of the sensor
    /// </summary>
    /// <returns>The current status of the sensor.</returns>
    public SensorStatus GetStatus()
    {
        if (communication == CommunicationType.I2C)
            return GetStatusI2c();
        return GetStatusSerial();
    }

    /// <summary>
    /// Get the target number
    /// </summary>
    public byte GetTargetNumber()
    {
        if (communication == CommunicationType.I2C)
            return GetTargetNumberI2c();
        return GetTargetNumberSerial();
    }

    /// <summary>
    /// Gets the target speed from the sensor's buffer.
    /// </summary>
    /// <returns>The target speed as a float.</returns>
    public Speed GetTargetSpeed()
    {
        return new Speed(motionData.Speed, Speed.UnitType.MetersPerSecond);
    }

    /// <summary>
    /// Gets the target range from the sensor's buffer.
    /// </summary>
    /// <returns>The target range as a float.</returns>
    public Length GetTargetRange()
    {
        return new Length(motionData.Range, Length.UnitType.Meters);
    }

    /// <summary>
    /// Gets the target energy from the sensor's buffer.
    /// </summary>
    /// <returns>The target energy as an unsigned integer.</returns>
    public uint GetTargetEnergy()
    {
        return motionData.Energy;
    }

    /// <summary>
    /// Determines if motion is currently detected by the sensor.
    /// </summary>
    /// <returns><c>true</c> if motion is detected, otherwise <c>false</c>.</returns>
    public bool IsMotionDetected()
    {
        if (communication == CommunicationType.I2C)
        {
            return IsMotionDetectedI2c();
        }
        else
        {
            return IsMotionDetectedSerial();
        }
    }

    /// <inheritdoc/>
    public bool SetDetectionRange(ushort min, ushort max, ushort trig)
    {
        if (communication == CommunicationType.I2C)
            return SetDetectionRangeI2c(min, max, trig);
        return SetDetectionRangeSerial(min, max, trig);
    }

    /// <inheritdoc/>
    public bool SetTrigSensitivity(byte sensitivity)
    {
        if (communication == CommunicationType.I2C)
            return SetTrigSensitivityI2c(sensitivity);
        return SetTrigSensitivitySerial(sensitivity);
    }

    /// <inheritdoc/>
    public bool SetKeepSensitivity(byte sensitivity)
    {
        if (communication == CommunicationType.I2C)
            return SetKeepSensitivityI2c(sensitivity);
        return SetKeepSensitivitySerial(sensitivity);
    }

    /// <inheritdoc/>
    public bool SetDelay(byte trig, ushort keep)
    {
        if (communication == CommunicationType.I2C)
            return SetDelayI2c(trig, keep);
        return SetDelaySerial(trig, keep);
    }

    /// <inheritdoc/>
    public ushort GetKeepTimeout()
    {
        if (communication == CommunicationType.I2C)
            return GetKeepTimeoutI2c();
        return GetKeepTimeoutSerial();
    }
}