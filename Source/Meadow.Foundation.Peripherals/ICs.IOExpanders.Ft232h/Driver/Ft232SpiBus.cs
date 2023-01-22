using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Ft232h;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public sealed class Ft232SpiBus : IFt232Bus, ISpiBus, IDisposable
    {
        public const uint DefaultClockRate = 100000;
        private const byte DefaultLatencyTimer = 10; // from the FTDI sample

        private bool _isDisposed;

        private SpiClockConfiguration _config;
        private SpiChannelConfig _channelConfig;

        public IntPtr Handle { get; private set; }
        internal bool IsOpen { get; private set; } = false;
        internal int ChannelNumber { get; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        public Frequency[] SupportedSpeeds => new Frequency[] { new Frequency(30d, Frequency.UnitType.Megahertz) };

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

        public SpiClockConfiguration Configuration
        {
            get { return _config; }
            set
            {
                _channelConfig = CreateChannelConfig(value);
                _config = value;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Open(SpiClockConfiguration config)
        {
            Configuration = config;

            if (CheckStatus(Functions.SPI_OpenChannel(ChannelNumber, out IntPtr handle)))
            {
                Handle = handle;

                CheckStatus(Functions.SPI_InitChannel(Handle, ref _channelConfig));
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

        private SpiChannelConfig CreateChannelConfig(SpiClockConfiguration config)
        {
            // for now we supprt CS on D3 and that's it
            Ft232h.SpiConfigOptions opts = SpiConfigOptions.CS_ACTIVELOW | SpiConfigOptions.CS_DBUS3;

            switch (config.SpiMode)
            {
                case SpiClockConfiguration.Mode.Mode0:
                    opts = SpiConfigOptions.MODE0;
                    break;
                case SpiClockConfiguration.Mode.Mode1:
                    opts = SpiConfigOptions.MODE1;
                    break;
                case SpiClockConfiguration.Mode.Mode2:
                    opts = SpiConfigOptions.MODE2;
                    break;
                case SpiClockConfiguration.Mode.Mode3:
                    opts = SpiConfigOptions.MODE3;
                    break;
            }

            return new SpiChannelConfig
            {
                ClockRate = (uint)config.Speed.Hertz,
                LatencyTimer = DefaultLatencyTimer,
                Options = opts
            };
        }

        private SPITransferOptions CreateTransferOptions(ChipSelectMode mode)
        {
            SPITransferOptions opts = SPITransferOptions.SIZE_IN_BYTES;

            switch (mode)
            {
                case ChipSelectMode.ActiveLow:
                    opts |= SPITransferOptions.CHIPSELECT_DISABLE;
                    break;
                case ChipSelectMode.ActiveHigh:
                    opts |= SPITransferOptions.CHIPSELECT_ENABLE;
                    break;
            }

            return opts;
        }

        public void Read(IDigitalOutputPort chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            var status = Functions.SPI_Read(
                Handle,
                MemoryMarshal.GetReference(readBuffer),
                readBuffer.Length,
                out _,
                options
                );

            CheckStatus(status);
        }

        public void Write(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            var status = Functions.SPI_Write(
                Handle,
                MemoryMarshal.GetReference(writeBuffer),
                writeBuffer.Length,
                out _,
                options
                );

            CheckStatus(status);
        }

        public void Exchange(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            var status = Functions.SPI_ReadWrite(
                Handle,
                MemoryMarshal.GetReference(readBuffer),
                MemoryMarshal.GetReference(writeBuffer),
                writeBuffer.Length,
                out _,
                options
                );

            CheckStatus(status);
        }
    }
}