namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Lis3mdl
    {
        /// <summary>
        /// Valid addresses for the sensor
        /// </summary>
        enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x1C
            /// </summary>
            Address_0x1C = 0x1C,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x1C
        }
    }
}