namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl345
    {
        /// <summary>
        /// Frequency of the sensor readings when the device is in sleep mode.
        /// </summary>
        /// <remarks>
        /// See page 26 of the data sheet
        /// </remarks>
        public enum Frequencies : byte
        {
            /// <summary>
            /// 8hz
            /// </summary>
            EightHz = 0x00,
            /// <summary>
            /// 4hz
            /// </summary>
            FourHz = 0x01,
            /// <summary>
            /// 2hz
            /// </summary>
            TwoHz = 0x02,
            /// <summary>
            /// 1hz
            /// </summary>
            OneHz = 0x03,
        }
    }
}
