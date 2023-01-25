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
        /// Display registers
        /// </summary>
        protected enum Register : byte
        {
            /// <summary>
            /// NO_OP
            /// </summary>
            NO_OP = 0x00,
            /// <summary>
            /// MADCTL
            /// </summary>
            MADCTL = 0x36,
            /// <summary>
            /// MADCTL_MY
            /// </summary>
            MADCTL_MY = 0x80,
            /// <summary>
            /// MADCTL_MX
            /// </summary>
            MADCTL_MX = 0x40,
            /// <summary>
            /// MADCTL_MV
            /// </summary>
            MADCTL_MV = 0x20,
            /// <summary>
            /// MADCTL_ML
            /// </summary>
            MADCTL_ML = 0x10,
            /// <summary>
            /// MADCTL_RGB
            /// </summary>
            MADCTL_RGB = 0x00,
            /// <summary>
            /// MADCTL_BGR
            /// </summary>
            MADCTL_BGR = 0x08,
            /// <summary>
            /// MADCTL_MH
            /// </summary>
            MADCTL_MH = 0x04,
            /// <summary>
            /// MADCTL_SS
            /// </summary>
            MADCTL_SS = 0x02,
            /// <summary>
            /// MADCTL_GS
            /// </summary>
            MADCTL_GS = 0x01,
            /// <summary>
            /// COLOR_MODE
            /// </summary>
            COLOR_MODE = 0x3A,
        }
    }
}