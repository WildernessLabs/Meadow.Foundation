namespace Meadow.Foundation.Sensors.Temperature;

public partial class Mlx90614
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x5A
        /// </summary>
        Address_0x5A = 0x5A,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x5A
    }
}
