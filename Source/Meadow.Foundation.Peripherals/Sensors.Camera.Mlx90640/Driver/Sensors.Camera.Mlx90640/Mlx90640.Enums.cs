namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Mlx90640
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x33
            /// </summary>
            Address_0x33 = 0x33,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x33
        }


        public enum Mode
        {
            Interleaved,
            Chess
        }

        public enum Resolution
        {
            SixteenBit,
            SeventeenBit,
            EighteenBit,
            NineteenBit
        }

        public enum RefreshRate
        {
            HalfHZ,
            OneHZ,
            TwoHZ,
            FourHZ,
            EightHZ,
            SixteenHZ,
            ThirtyTwoHZ,
            SixtyFourHZ
        }

        public enum Units
        {
            Celsius,
            Fahrenheit,
            Kelvin
        }
    }
}
