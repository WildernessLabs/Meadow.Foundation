namespace Meadow.Foundation.Sensors.Atmospheric.Dhtxx
{
    /// <summary>
    /// Represents a DHT10 temp / humidity sensor
    /// -40 - 80 celius +/- 0.5
    /// 0 - 99.9% humidity +/- 3%
    /// </summary>
    public abstract partial class DhtBase
    {
        /// <summary>
		///     Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x5C
            /// </summary>
            Address_0x5C = 0x5C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x5C
        }

        private enum BusType
        {
            I2C,
            OneWire,
        }
    }
}
