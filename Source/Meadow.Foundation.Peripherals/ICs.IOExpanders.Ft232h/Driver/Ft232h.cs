using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FT232 IO Expander
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

    internal bool UsingMpsse { get; }

    /// <summary>
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ft232h"/> class.
    /// </summary>
    /// <param name="useMPSSE">Specifies whether to use Multi-Protocol Synchronous Serial Engine (MPSSE) mode (default is false).</param>
    public Ft232h(bool useMPSSE = false)
    {
        UsingMpsse = useMPSSE;

        _impl = UsingMpsse ? new MpsseImpl() : new Ftd2xxImpl();
        _impl.Initialize();

        Pins = new PinDefinitions(this);
    }

    /// <inheritdoc/>
    public II2cBus CreateI2cBus(int busNumber = 0)
    {
        return CreateI2cBus(busNumber, I2CClockRate.Standard);
    }

    /// <inheritdoc/>
    public II2cBus CreateI2cBus(int busNumber, I2cBusSpeed busSpeed)
    {
        // TODO: convert frequency
        return CreateI2cBus(busNumber, I2CClockRate.Standard);
    }

    /// <inheritdoc/>
    public II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed)
    {
        // TODO: map the pins to the bus number
        // TODO: convert frequency
        return CreateI2cBus(0, I2CClockRate.Standard);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus()
    {
        return CreateSpiBus(0, DefaultClockConfiguration);
    }

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
    {
        if (!clock.Supports<ISpiChannelInfo>(c => c.LineTypes.HasFlag(SpiLineType.Clock)))
        {
            throw new ArgumentException("Invalid Clock line");
        }

        // TODO: map the pins to the bus number
        return CreateSpiBus(0, config);
    }

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, Frequency speed)
    {
        // TODO: map the pins to the bus number
        var config = new SpiClockConfiguration(speed);
        return CreateSpiBus(0, config);
    }

    /// <inheritdoc/>
    public ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config)
    {
        return _impl.CreateSpiBus(busNumber, config);
    }

    /// <inheritdoc/>
    public static SpiClockConfiguration DefaultClockConfiguration
    {
        get => new SpiClockConfiguration(
             new Frequency(MpsseSpiBus.DefaultClockRate, Frequency.UnitType.Hertz));
    }

    /// <inheritdoc/>
    public IDigitalInputPort CreateDigitalInputPort(IPin pin)
    {
        return CreateDigitalInputPort(pin, ResistorMode.Disabled);
    }

    /// <inheritdoc/>
    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
    {
        // TODO: need to select the proper channel based on pin
        return _impl.CreateDigitalInputPort(0, pin, resistorMode);
    }

    /// <inheritdoc/>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        // TODO: need to select the proper channel based on pin
        return _impl.CreateDigitalOutputPort(0, pin, initialState, initialOutputType);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _impl.Dispose();

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Finalizer for the Ft232h class, used to release unmanaged resources.
    /// </summary>
    ~Ft232h()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}