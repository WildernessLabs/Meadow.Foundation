namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// Color interrupt channels
        /// </summary>
        public enum InterruptChannels : byte
        {
            /// <summary>
            /// Red
            /// </summary>
            Red = 0x0,
            /// <summary>
            /// Green
            /// </summary>
            Green = 0x1,
            /// <summary>
            /// Blue
            /// </summary>
            Blue = 0x2,
            /// <summary>
            /// Clear
            /// </summary>
            Clear = 0x3
        }
    }
}
