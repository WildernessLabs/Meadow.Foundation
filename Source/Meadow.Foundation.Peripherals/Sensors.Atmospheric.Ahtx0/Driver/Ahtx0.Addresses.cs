namespace Meadow.Foundation.Sensors.Atmospheric;

public partial class Ahtx0
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x38
        /// </summary>
        Address_0x38 = 0x38,
        /// <summary>
        /// Bus address 0x39
        /// </summary>
        Address_0x39 = 0x39,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x38
    }
}
