using System;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal class I2cChannel
    {
        private const byte DefaultLatencyTimer = 10;
        private const int DefaultChannelOptions = 0;

        public int ChannelNumber { get; }
        protected IntPtr Handle { get; set; }

        private FT_DEVICE_LIST_INFO_NODE _infoNode;

        internal I2cChannel(int channelNumber, FT_DEVICE_LIST_INFO_NODE infoNode)
        {
            _infoNode = infoNode;
            ChannelNumber = channelNumber;
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

        internal void CloseChannel()
        {
            if (Handle != IntPtr.Zero)
            {
                CheckStatus(Functions.I2C_CloseChannel(Handle));
                Handle = IntPtr.Zero;
            }
        }
    }
}