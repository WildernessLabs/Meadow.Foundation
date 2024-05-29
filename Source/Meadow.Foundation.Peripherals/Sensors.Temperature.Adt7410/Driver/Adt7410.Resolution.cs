namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Adt7410
    {
        /// <summary>
        /// Indicate the resolution of the sensor
        /// </summary>
        public enum Resolution : byte
        {
            /// <summary>
            /// Operate in 16-bit mode
            /// </summary>
            Resolution16Bits,

            /// <summary>
            /// Operate in 13-bit mode
            /// </summary>
            Resolution13Bits
        }
    }
}