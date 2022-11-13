namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mma7660fc
    {
        /// <summary>
        /// MMA7660FC registers
        /// </summary>
        public enum Registers : byte
        {
            /// <summary>
            /// X out
            /// </summary>
            XOUT = 0x00,
            /// <summary>
            /// Y out
            /// </summary>
            YOUT = 0x01,
            /// <summary>
            /// Z out
            /// </summary>
            ZOUT = 0x02,
            /// <summary>
            /// Tilt
            /// </summary>
            TILT = 0x03,
            /// <summary>
            /// Mode
            /// </summary>
            Mode = 0x07,
            /// <summary>
            /// Sleep rate
            /// </summary>
            SleepRate = 0x08
        }
    }
}
