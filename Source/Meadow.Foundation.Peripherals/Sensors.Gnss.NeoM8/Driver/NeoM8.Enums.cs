namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x42
            /// </summary>
            Address_0x42 = 0x42,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x42
        }

        enum CommunicationMode
        {
            Serial,
            SPI,
            I2C,
        }
    }
}