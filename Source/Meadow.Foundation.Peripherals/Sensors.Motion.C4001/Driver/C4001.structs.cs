namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    /// <summary>
    /// Represents the current status of the C4001 sensor.
    /// </summary>
    public struct SensorStatus
    {
        /// <summary>
        /// Indicates the sensor's work status (0 = stop, 1 = start).
        /// </summary>
        public byte WorkStatus;
        /// <summary>
        /// Indicates the sensor's work mode (0 = presence, 1 = speed).
        /// </summary>
        public byte WorkMode;
        /// <summary>
        /// Indicates the sensor's initialization status (0 = not initialized, 1 = initialization successful).
        /// </summary>
        public byte InitStatus;
    }

    internal struct SensorMotionData
    {
        public byte Number;
        public float Speed;
        public float Range;
        public uint Energy;
    }

    internal struct ResponseData
    {
        public bool Status;
        public float Response1;
        public float Response2;
        public float Response3;
    }

    internal struct PwmData
    {
        public byte Pwm1;
        public byte Pwm2;
        public byte Timer;
    }

    internal struct AllData
    {
        public byte Exist;
        public SensorStatus Status;
        public SensorMotionData Target;
    }
}