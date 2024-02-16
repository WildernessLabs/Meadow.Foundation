using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

internal class Ftd2xxImpl : IFtdiImpl
{
    private FtdiDeviceCollection _devices = default!;

    public void Initialize()
    {
        _devices = new FtdiDeviceCollection();
        _devices.Refresh();
    }

    public II2cBus CreateI2cBus(int channel, I2CClockRate clock)
    {
        if (_devices.Count == 0)
        {
            throw new DeviceNotFoundException();
        }

        _devices[channel].Open();

        return new Ft23xxI2cBus(_devices[channel]);
    }

    public ISpiBus CreateSpiBus(int channel, SpiClockConfiguration config)
    {
        if (_devices.Count == 0)
        {
            throw new DeviceNotFoundException();
        }

        _devices[channel].Open();

        return new Ft23xxSpiBus(_devices[channel], config);
    }

    public IDigitalInputPort CreateDigitalInputPort(int channel, IPin pin, ResistorMode resistorMode)
    {
        if (_devices.Count == 0)
        {
            throw new DeviceNotFoundException();
        }

        return new Ft23xxDigitalInputPort(_devices[channel], pin, resistorMode,
            new DigitalChannelInfo(pin.Name, true, true, false, false, false, false));
    }

    public IDigitalOutputPort CreateDigitalOutputPort(int channel, IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        if (_devices.Count == 0)
        {
            throw new DeviceNotFoundException();
        }

        return new Ft23xxDigitalOutputPort(_devices[channel], pin,
            new DigitalChannelInfo(pin.Name, true, true, false, false, false, false),
            initialState, initialOutputType);
    }

    public void Dispose()
    {
        foreach (var d in _devices)
        {
            d.Close();
        }
    }
}
