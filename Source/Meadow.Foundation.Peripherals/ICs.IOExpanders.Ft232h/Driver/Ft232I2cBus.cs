using Meadow.Hardware;
using Meadow.Units;
using System;
using static Meadow.Foundation.ICs.IOExpanders.Ft232h;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Ft232SpiBus : ISpiBus, IDisposable
    {
        public const uint DefaultClockRate = 100000;
        private const byte DefaultLatencyTimer = 10; // from the FTDI sample

        private bool _isDisposed;

        internal bool IsOpen { get; private set; } = false;

        public int ChannelNumber { get; }
        private IntPtr Handle { get; set; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        internal Ft232SpiBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
        {
            ChannelNumber = channelNumber;
            InfoNode = info;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                CloseChannel();

                _isDisposed = true;
            }
        }

        ~Ft232SpiBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Open(SpiConfigOption options, uint clockRate = DefaultClockRate)
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

        private void CloseChannel()
        {
            if (Handle != IntPtr.Zero)
            {
                CheckStatus(Functions.SPI_CloseChannel(Handle));
                Handle = IntPtr.Zero;
            }
        }




        public Frequency[] SupportedSpeeds => throw new NotImplementedException();
        public SpiClockConfiguration Configuration => throw new NotImplementedException();

        public void Read(IDigitalOutputPort chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            throw new NotImplementedException();
        }

        public void Write(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            throw new NotImplementedException();
        }

        public void Exchange(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class Ft232I2cBus : II2cBus, IDisposable
    {
        private const byte DefaultLatencyTimer = 10;
        private const int DefaultChannelOptions = 0;

        private bool _isDisposed;

        internal bool IsOpen { get; private set; } = false;

        public int ChannelNumber { get; }
        private IntPtr Handle { get; set; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        internal Ft232I2cBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
        {
            ChannelNumber = channelNumber;
            InfoNode = info;
        }

        public Frequency Frequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                CloseChannel();

                _isDisposed = true;
            }
        }

        ~Ft232I2cBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Open(I2CClockRate clockRate = I2CClockRate.Standard)
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

                IsOpen = true;
            }
        }

        private void CloseChannel()
        {
            if (Handle != IntPtr.Zero)
            {
                CheckStatus(Functions.I2C_CloseChannel(Handle));
                Handle = IntPtr.Zero;
            }
        }

        public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        public void Read(byte peripheralAddress, Span<byte> readBuffer)
        {
            throw new NotImplementedException();
        }

        public void Write(byte peripheralAddress, Span<byte> writeBuffer)
        {
            throw new NotImplementedException();
        }
    }
}