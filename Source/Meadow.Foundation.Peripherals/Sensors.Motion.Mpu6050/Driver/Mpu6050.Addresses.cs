namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mpu6050
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            ///  Bus address 0x68
            /// </summary>
            Address_0x68 = 0x68,
            /// <summary>
            ///  Bus address 0x69
            /// </summary>
            Address_0x69 = 0x69,
            /// <summary>
            ///  Bus address 0x68
            /// </summary>
            Default = Address_0x68
        }
    }
}
