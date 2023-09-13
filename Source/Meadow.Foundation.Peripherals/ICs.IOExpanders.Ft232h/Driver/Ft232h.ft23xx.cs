using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

internal interface IFtdiImpl : IDisposable
{
    void Initialize();
    II2cBus CreateI2cBus(int busNumber, I2CClockRate clock);
    ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config);
    IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode);
    IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull);
}

internal class Ftd2xxImpl : IFtdiImpl
{
    private FtdiDeviceCollection _devices = default!;

    public void Initialize()
    {
        _devices = new FtdiDeviceCollection();
        _devices.Refresh();
    }

    public II2cBus CreateI2cBus(int busNumber, I2CClockRate clock)
    {
        _devices[busNumber].Open();

        return new Ft23xxI2cBus(_devices[busNumber]);
    }

    public ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config)
    {
        throw new NotImplementedException();
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        throw new NotImplementedException();
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        foreach (var d in _devices)
        {
            d.Close();
        }
    }
}
