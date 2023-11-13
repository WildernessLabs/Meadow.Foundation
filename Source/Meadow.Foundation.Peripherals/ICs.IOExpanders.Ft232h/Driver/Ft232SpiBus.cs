using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Ft232h;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an SPI bus using the FT232H USB to SPI bridge.
    /// </summary>
    public sealed class Ft232SpiBus : IFt232Bus, ISpiBus, IDisposable
    {
        /// <summary>
        /// The default clock rate for the FT232 SPI bus.
        /// </summary>
        public const uint DefaultClockRate = 25000000;

        private const byte DefaultLatencyTimer = 10; // from the FTDI sample

        private bool _isDisposed;

        private SpiClockConfiguration _config;
        private SpiChannelConfig _channelConfig;

        /// <summary>
        /// Gets the handle for the FT232H SPI bus.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Gets or sets the GPIO direction mask for the FT232H SPI bus.
        /// </summary>
        public byte GpioDirectionMask { get; set; }

        /// <summary>
        /// Gets or sets the GPIO state for the FT232H SPI bus.
        /// </summary>
        public byte GpioState { get; set; }

        internal bool IsOpen { get; private set; } = false;
        internal int ChannelNumber { get; }
        private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

        /// <summary>
        /// Gets the supported SPI bus speeds for the FT232H SPI bus.
        /// </summary>
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

        /// <summary>
        /// Finalizes an instance of the <see cref="Ft232SpiBus"/> class.
        /// </summary>
        ~Ft232SpiBus()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
            // For now, we support CS on D3 and that's it
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

        /// <summary>
        /// Reads data from a device on the SPI bus.
        /// </summary>
        /// <param name="chipSelect">The digital output port representing the chip select line.</param>
        /// <param name="readBuffer">The buffer to store the read data.</param>
        /// <param name="csMode">The chip select mode (active low or active high).</param>
        public void Read(IDigitalOutputPort chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;

            var status = Functions.SPI_Read(
                Handle,
                MemoryMarshal.GetReference(readBuffer),
                readBuffer.Length,
                out _,
                options
            );

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;

            CheckStatus(status);
        }

        /// <summary>
        /// Writes data to a device on the SPI bus.
        /// </summary>
        /// <param name="chipSelect">The digital output port representing the chip select line.</param>
        /// <param name="writeBuffer">The data to write to the device.</param>
        /// <param name="csMode">The chip select mode (active low or active high).</param>
        public void Write(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;

            var status = Functions.SPI_Write(
                Handle,
                MemoryMarshal.GetReference(writeBuffer),
                writeBuffer.Length,
                out _,
                options
            );

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;

            CheckStatus(status);
        }

        /// <summary>
        /// Exchanges data with a device on the SPI bus.
        /// </summary>
        /// <param name="chipSelect">The digital output port representing the chip select line.</param>
        /// <param name="writeBuffer">The data to write to the device.</param>
        /// <param name="readBuffer">The buffer to store the read data.</param>
        /// <param name="csMode">The chip select mode (active low or active high).</param>
        public void Exchange(IDigitalOutputPort chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            var options = CreateTransferOptions(csMode);

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;

            var status = Functions.SPI_ReadWrite(
                Handle,
                MemoryMarshal.GetReference(readBuffer),
                MemoryMarshal.GetReference(writeBuffer),
                writeBuffer.Length,
                out _,
                options
            );

            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;

            CheckStatus(status);
        }

        /// <summary>
        /// Gets or sets the SPI bus configuration.
        /// </summary>
        public SpiClockConfiguration Configuration
        {
            get { return _config; }
            set
            {
                _channelConfig = CreateChannelConfig(value);
                _config = value;
                this.Configuration.Changed += OnConfigurationChanged;
            }
        }

        private void OnConfigurationChanged(object sender, EventArgs e)
        {
            var changed = false;

            if (Configuration.Speed.Hertz != _channelConfig.ClockRate)
            {
                _channelConfig.ClockRate = (uint)Configuration.Speed.Hertz;
                changed = true;
            }

            switch (Configuration.SpiMode)
            {
                case SpiClockConfiguration.Mode.Mode0:
                    if ((_channelConfig.Options & SpiConfigOptions.MODE3) != SpiConfigOptions.MODE0)
                    {
                        _channelConfig.Options |= SpiConfigOptions.MODE0;
                        changed = true;
                    }
                    break;
                case SpiClockConfiguration.Mode.Mode1:
                    if ((_channelConfig.Options & SpiConfigOptions.MODE3) != SpiConfigOptions.MODE1)
                    {
                        _channelConfig.Options = (_channelConfig.Options & ~SpiConfigOptions.MODE3) | SpiConfigOptions.MODE1;
                        changed = true;
                    }
                    break;
                case SpiClockConfiguration.Mode.Mode2:
                    if ((_channelConfig.Options & SpiConfigOptions.MODE3) != SpiConfigOptions.MODE2)
                    {
                        _channelConfig.Options = (_channelConfig.Options & ~SpiConfigOptions.MODE3) | SpiConfigOptions.MODE2;
                        changed = true;
                    }
                    break;
                case SpiClockConfiguration.Mode.Mode3:
                    if ((_channelConfig.Options & SpiConfigOptions.MODE3) != SpiConfigOptions.MODE3)
                    {
                        _channelConfig.Options = (_channelConfig.Options & ~SpiConfigOptions.MODE3) | SpiConfigOptions.MODE3;
                        changed = true;
                    }
                    break;
            }

            if (changed)
            {
                CheckStatus(Functions.SPI_InitChannel(Handle, ref _channelConfig));
            }
        }
    }
}
