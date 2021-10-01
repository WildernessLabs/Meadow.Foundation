namespace Meadow.Foundation.Sensors.Camera
{
    public partial class Mlx90640
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x33,
            Default = Address0
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
