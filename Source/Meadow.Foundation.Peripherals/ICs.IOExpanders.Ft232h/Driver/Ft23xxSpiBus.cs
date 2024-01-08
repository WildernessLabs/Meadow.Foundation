using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an SPI bus implementation using the FT23xx device.
/// </summary>
public sealed class Ft23xxSpiBus : IFt232Bus, ISpiBus, IDisposable
{
    private FtdiDevice _device;

    /// <summary>
    /// Gets the handle to the FT23xx device used by the SPI bus.
    /// </summary>
    public IntPtr Handle => _device.Handle;

    /// <inheritdoc/>
    public byte GpioDirectionMask { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <inheritdoc/>
    public byte GpioState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <inheritdoc/>
    public Frequency[] SupportedSpeeds => throw new NotImplementedException();

    /// <inheritdoc/>
    public SpiClockConfiguration Configuration { get; }

    internal Ft23xxSpiBus(FtdiDevice device, SpiClockConfiguration config)
    {
        Configuration = config;

        if (device.Handle == IntPtr.Zero)
        {
            device.Open();
        }

        _device = device;

        _device.InitializeSpi(Configuration);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        _device.SpiExchange(chipSelect, writeBuffer, readBuffer, csMode);
    }

    /// <inheritdoc/>
    public void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        _device.SpiRead(chipSelect, readBuffer, csMode);
    }

    /// <inheritdoc/>
    public void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode)
    {
        _device.SpiWrite(chipSelect, writeBuffer, csMode);
    }
}
