using System;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        public class I2cChannel : Channel
        {
            private const byte DefaultLatencyTimer = 10;
            private const int DefaultChannelOptions = 0;

            internal I2cChannel(int channelNumber, FT_DEVICE_LIST_INFO_NODE infoNode)
                : base(channelNumber, infoNode)
            {
            }

            public void Open(I2CClockRate clockRate = I2CClockRate.Standard)
            {
                if (CheckStatus(Functions.I2C_OpenChannel(ChannelNumber, out IntPtr handle)))
                {
                    Handle = handle;

                    var config = new I2CChannelConfig
                    {
                        ClockRate = clockRate,
                        LatencyTimer = DefaultLatencyTimer,
                        Options = DefaultChannelOptions
                    };

                    CheckStatus(Functions.I2C_InitChannel(Handle, ref config));
                }
            }

            protected override void CloseChannel()
            {
                if (Handle != IntPtr.Zero)
                {
                    CheckStatus(Functions.I2C_CloseChannel(Handle));
                    Handle = IntPtr.Zero;
                }
            }
        }
    }
}