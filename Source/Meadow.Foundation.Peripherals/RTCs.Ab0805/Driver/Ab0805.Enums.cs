namespace Meadow.Foundation.RTCs;

public partial class Ab0805
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x69
        /// </summary>
        Address_0x69 = 0x69,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x69
    }

    /// <summary>
    /// Represents the unit of delay time
    /// </summary>
    public enum DelayTimeUnit
    {
        /// <summary>
        /// Delay time in seconds
        /// </summary>
        Seconds,
        /// <summary>
        /// Delay time in minutes
        /// </summary>
        Minutes,
    }
}