namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Tmp102
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x48
            /// </summary>
            Address_0x48 = 0x48,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x48
        }
    }
}