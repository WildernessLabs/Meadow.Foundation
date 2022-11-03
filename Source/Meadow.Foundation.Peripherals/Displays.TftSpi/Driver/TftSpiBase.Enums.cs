namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents an abstract color TFT display using SPI communication
    /// </summary>
    public abstract partial class TftSpiBase
    {
        /// <summary>
        /// TFT SPI commands
        /// </summary>
        protected enum LcdCommand
        {
            /// <summary>
            /// CASET
            /// </summary>
            CASET = 0x2A,
            /// <summary>
            /// RASET
            /// </summary>
            RASET = 0x2B,
            /// <summary>
            /// RAMWR
            /// </summary>
            RAMWR = 0x2C,
            /// <summary>
            /// RADRD
            /// </summary>
            RADRD = 0x2E
        };

        /// <summary>
        /// The display rotation
        /// </summary>
        public enum Rotation
        {
            /// <summary>
            /// Normal / default rotation
            /// </summary>
            Normal, //zero
            /// <summary>
            /// Rotate 90 degrees
            /// </summary>
            Rotate_90,
            /// <summary>
            /// Rotate 180 degrees
            /// </summary>
            Rotate_180,
            /// <summary>
            /// Rotate 270 degrees
            /// </summary>
            Rotate_270,
        }

        /// <summary>
        /// Display registers
        /// </summary>
        internal enum Register : byte
        {
            NO_OP = 0x0,
            MADCTL = 0x36,
            MADCTL_MY = 0x80,
            MADCTL_MX = 0x40,
            MADCTL_MV = 0x20,
            MADCTL_ML = 0x10,
            MADCTL_RGB = 0x00,
            MADCTL_BGR = 0X08,
            MADCTL_MH = 0x04,
            MADCTL_SS = 0x02,
            MADCTL_GS = 0x01,
            COLOR_MODE = 0x3A,
        }
    }
}