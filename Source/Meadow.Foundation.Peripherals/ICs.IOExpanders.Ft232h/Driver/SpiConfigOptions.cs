using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h_old
    {
        /// <summary>
        /// Flags for configuring the SPI communication options.
        /// </summary>
        [Flags]
        public enum SpiConfigOptions
        {
            /// <summary>
            /// SPI Mode 0: CPOL=0, CPHA=0.
            /// </summary>
            MODE0 = 0x00000000,

            /// <summary>
            /// SPI Mode 1: CPOL=0, CPHA=1.
            /// </summary>
            MODE1 = 0x00000001,

            /// <summary>
            /// SPI Mode 2: CPOL=1, CPHA=0.
            /// </summary>
            MODE2 = 0x00000002,

            /// <summary>
            /// SPI Mode 3: CPOL=1, CPHA=1.
            /// </summary>
            MODE3 = 0x00000003,

            /// <summary>
            /// Chip Select on D3.
            /// </summary>
            CS_DBUS3 = 0x00000000,

            /// <summary>
            /// Chip Select on D4.
            /// </summary>
            CS_DBUS4 = 0x00000004,

            /// <summary>
            /// Chip Select on D5.
            /// </summary>
            CS_DBUS5 = 0x00000008,

            /// <summary>
            /// Chip Select on D6.
            /// </summary>
            CS_DBUS6 = 0x0000000C,

            /// <summary>
            /// Chip Select on D7.
            /// </summary>
            CS_DBUS7 = 0x00000010,

            /// <summary>
            /// Chip Select active high.
            /// </summary>
            CS_ACTIVEHIGH = 0x00000000,

            /// <summary>
            /// Chip Select active low.
            /// </summary>
            CS_ACTIVELOW = 0x00000020
        }
    }
}
