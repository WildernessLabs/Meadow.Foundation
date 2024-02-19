using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Ft232h;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an SPI bus implementation using the FT232 device.
/// </summary>
public sealed class MpsseSpiBus : IFt232Bus, ISpiBus, IDisposable
{
    /// <summary>
    /// The default SPI clock rate for the FT232H
    /// </summary>
    public const uint DefaultClockRate = 25000000;
    private const byte DefaultLatencyTimer = 10; // from the FTDI sample

    private bool _isDisposed;

    private SpiClockConfiguration _config = default!;
    private SpiChannelConfig _channelConfig;

    /// <inheritdoc/>
    public IntPtr Handle { get; private set; }
    /// <inheritdoc/>
    public byte GpioDirectionMask { get; set; }
    /// <inheritdoc/>
    public byte GpioState { get; set; }
    internal bool IsOpen { get; private set; } = false;
    internal int ChannelNumber { get; }
    private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

    /// <inheritdoc/>
    public Frequency[] SupportedSpeeds => new Frequency[] { new Frequency(30d, Frequency.UnitType.Megahertz) };

    internal MpsseSpiBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
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
    /// Finalizer for the Ft232SpiBus class, used to release unmanaged resources.
    /// </summary>
    ~MpsseSpiBus()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }

    /// <inheritdoc/>
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
            CheckStatus(Mpsse.SPI_InitChannel(Handle, ref _channelConfig));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal void Open(SpiClockConfiguration config)
    {
        Configuration = config;

        if (CheckStatus(Mpsse.SPI_OpenChannel(ChannelNumber, out IntPtr handle)))
        {
            Handle = handle;

            CheckStatus(Mpsse.SPI_InitChannel(Handle, ref _channelConfig));
        }
    }

    private void CloseChannel()
    {
        if (Handle != IntPtr.Zero)
        {
            CheckStatus(Mpsse.SPI_CloseChannel(Handle));
            Handle = IntPtr.Zero;
        }
    }

    private SpiChannelConfig CreateChannelConfig(SpiClockConfiguration config)
    {
        // for now we support CS on D3 and that's it
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

    /// <inheritdoc/>
    public void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
    {
        var options = CreateTransferOptions(csMode);

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
        }

        var status = Mpsse.SPI_Read(
            Handle,
            MemoryMarshal.GetReference(readBuffer),
            readBuffer.Length,
            out _,
            options
            );

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
        }

        CheckStatus(status);
    }

    /// <inheritdoc/>
    public void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
    {
        var options = CreateTransferOptions(csMode);

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
        }

        var status = Mpsse.SPI_Write(
            Handle,
            MemoryMarshal.GetReference(writeBuffer),
            writeBuffer.Length,
            out _,
            options
            );

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
        }

        CheckStatus(status);
    }

    /// <inheritdoc/>
    public void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow)
    {
        var options = CreateTransferOptions(csMode);

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? false : true;
        }

        var status = Mpsse.SPI_ReadWrite(
            Handle,
            MemoryMarshal.GetReference(readBuffer),
            MemoryMarshal.GetReference(writeBuffer),
            writeBuffer.Length,
            out _,
            options
            );

        if (chipSelect != null)
        {
            chipSelect.State = csMode == ChipSelectMode.ActiveLow ? true : false;
        }

        CheckStatus(status);
    }
}