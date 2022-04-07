namespace Meadow.Foundation.Leds
{
    public partial class SparkFunQwiicLEDStick
    {
        /// <summary>
		/// Valid addresses for the sensor.
		/// </summary>
		public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x22
            /// </summary>
            Address_0x22 = 0x22,
            /// <summary>
            /// Bus address 0x23
            /// </summary>
            Address_0x23 = 0x23,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x23
        }

        private enum Register
        {
            LedLength = 0x70,
            SingleColor = 0x71,
            AllColor = 0x72,
            RedArray = 0x73,
            GreenArray = 0x74,
            BlueArray = 0x75,
            SingleBrightness = 0x76,
            AllBrightness = 0x77,
            AllOff = 0x78,
            ChangeAddress = 0xC7,
        }
    }
}