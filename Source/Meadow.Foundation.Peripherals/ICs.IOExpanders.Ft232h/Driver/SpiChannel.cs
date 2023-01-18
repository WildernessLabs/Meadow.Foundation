using System;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        public class SpiChannel : Channel
        {
            public const uint DefaultClockRate = 100000;
            private const byte DefaultLatencyTimer = 10; // from the FTDI sample

            internal SpiChannel(int channelNumber, FT_DEVICE_LIST_INFO_NODE infoNode)
                : base(channelNumber, infoNode)
            {
            }

            public void Open(SpiConfigOption options, uint clockRate = DefaultClockRate)
            {
                //channelConf.configOptions = SPI_CONFIG_OPTION_MODE0 | SPI_CONFIG_OPTION_CS_DBUS3 | SPI_CONFIG_OPTION_CS_ACTIVELOW;

                if (CheckStatus(Functions.SPI_OpenChannel(ChannelNumber, out IntPtr handle)))
                {
                    Handle = handle;

                    var config = new SpiChannelConfig
                    {
                        ClockRate = clockRate,
                        LatencyTimer = DefaultLatencyTimer,
                        Options = options
                    };

                    CheckStatus(Functions.SPI_InitChannel(Handle, ref config));
                }
            }

            protected override void CloseChannel()
            {
                if (Handle != IntPtr.Zero)
                {
                    CheckStatus(Functions.SPI_CloseChannel(Handle));
                    Handle = IntPtr.Zero;
                }
            }
        }
    }
}