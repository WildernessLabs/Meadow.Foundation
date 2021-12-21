namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1750
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Address of the peripheral when the address pin is pulled low.
            /// </summary>
            Address0 = 0x5C,
            /// <summary>
            /// Address of the peripheral when the address pin is pulled high.
            /// </summary>
            Address1 = 0x23,
            Default = Address0
        }

    }
}
