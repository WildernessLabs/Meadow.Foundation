using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        [Flags]
        public enum SpiConfigOptions
        {
            MODE0 = 0x00000000,
            MODE1 = 0x00000001,
            MODE2 = 0x00000002,
            MODE3 = 0x00000003,
            CS_DBUS3 = 0x00000000,  /* 000 00 */
            CS_DBUS4 = 0x00000004,  /* 001 00 */
            CS_DBUS5 = 0x00000008,  /* 010 00 */
            CS_DBUS6 = 0x0000000C,  /* 011 00 */
            CS_DBUS7 = 0x00000010,  /* 100 00 */
            CS_ACTIVEHIGH = 0x00000000,
            CS_ACTIVELOW = 0x00000020
        }
    }
}