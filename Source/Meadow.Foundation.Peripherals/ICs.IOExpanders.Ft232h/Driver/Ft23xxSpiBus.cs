using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public sealed class Ft23xxSpiBus : IFt232Bus, ISpiBus, IDisposable
{
    private FtdiDevice _device;

    public IntPtr Handle => _device.Handle;
    public byte GpioDirectionMask { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public byte GpioState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Frequency[] SupportedSpeeds => throw new NotImplementedException();

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

    public void Dispose()
    {
    }

    public void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        _device.SpiExchange(chipSelect, writeBuffer, readBuffer, csMode);
    }

    public void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode)
    {
        _device.SpiRead(chipSelect, readBuffer, csMode);
    }

    public void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode)
    {
        _device.SpiWrite(chipSelect, writeBuffer, csMode);
    }
}
