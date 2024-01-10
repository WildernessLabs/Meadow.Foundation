using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an I2C bus implementation using the Cp2112 device.
/// </summary>
public sealed class Cp2112I2cBus : II2cBus, IDisposable
{
    private bool _isDisposed;
    private Cp2112 _device;

    internal Cp2112I2cBus(Cp2112 device, I2cBusSpeed busSpeed)
    {
        BusSpeed = busSpeed;
        _device = device;
    }

    /// <inheritdoc/>
    public I2cBusSpeed BusSpeed { get; set; }

    private void Dispose(bool _)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Finalizer for the Cp2112I2cBus class, used to release unmanaged resources.
    /// </summary>
    ~Cp2112I2cBus()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
    {
        Write(peripheralAddress, writeBuffer);
        Read(peripheralAddress, readBuffer);
    }

    /// <inheritdoc/>
    public void Read(byte peripheralAddress, Span<byte> readBuffer)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Write(byte peripheralAddress, Span<byte> writeBuffer)
    {
        _device.I2CWrite(peripheralAddress, writeBuffer);
    }
}