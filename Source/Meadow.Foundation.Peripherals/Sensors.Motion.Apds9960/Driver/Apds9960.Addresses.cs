namespace Meadow.Foundation.Sensors.Motion
{
    // TODO: the light stuff seems to work. not sure on the RGB conversion though.
    //  haven't tested any of the gesture stuff.
    //  need to add distance

    public partial class Apds9960
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x39
            /// </summary>
            Address_0x39 = 0x39,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x39
        }
    }
}