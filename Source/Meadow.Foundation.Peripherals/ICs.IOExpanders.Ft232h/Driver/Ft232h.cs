using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a DS3502 digital potentiometer
/// </summary>
public partial class Ft232h :
    IDisposable,
    IDigitalInputOutputController,
    IDigitalOutputController,
    ISpiController,
    II2cController
{
    private bool _isDisposed;
    private IFtdiImpl _impl;

    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    public Ft232h(bool useMPSSE = false)
    {
        _impl = useMPSSE ? new MpsseImpl() : new Ftd2xxImpl();
        _impl.Initialize();

        Pins = new PinDefinitions(this);
    }

    public II2cBus CreateI2cBus(int busNumber = 0)
    {
        return CreateI2cBus(busNumber, I2CClockRate.Standard);
    }

    public II2cBus CreateI2cBus(int busNumber, I2cBusSpeed busSpeed)
    {
        // TODO: convert frequency
        return CreateI2cBus(busNumber, I2CClockRate.Standard);
    }

    public II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed)
    {
        // TODO: map the pins to the bus number
        // TODO: convert frequency
        return CreateI2cBus(0, I2CClockRate.Standard);
    }

    public II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed)
    {
        // TODO: map the pins to the bus number
        // TODO: convert frequency
        return CreateI2cBus(0, I2CClockRate.Standard);
    }

    private II2cBus CreateI2cBus(int busNumber, I2CClockRate clock)
    {
        return _impl.CreateI2cBus(busNumber, clock);
    }

    public ISpiBus CreateSpiBus()
    {
        return CreateSpiBus(0, DefaultClockConfiguration);
    }

    public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
    {
        if (!clock.Supports<ISpiChannelInfo>(c => c.LineTypes.HasFlag(SpiLineType.Clock)))
        {
            throw new ArgumentException("Invalid Clock line");
        }

        // TODO: map the pins to the bus number
        return CreateSpiBus(0, config);
    }

    public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, Frequency speed)
    {
        // TODO: map the pins to the bus number
        var config = new SpiClockConfiguration(speed);
        return CreateSpiBus(0, config);
    }

    public ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config)
    {
        return _impl.CreateSpiBus(busNumber, config);
    }

    public static SpiClockConfiguration DefaultClockConfiguration
    {
        get => new SpiClockConfiguration(
             new Frequency(Ft232SpiBus.DefaultClockRate, Frequency.UnitType.Hertz));
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin)
    {
        return CreateDigitalInputPort(pin, ResistorMode.Disabled);
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        return _impl.CreateDigitalInputPort(pin, resistorMode);
    }

    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        return _impl.CreateDigitalOutputPort(pin, initialState, initialOutputType);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _impl.Dispose();

            _isDisposed = true;
        }
    }

    ~Ft232h()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}