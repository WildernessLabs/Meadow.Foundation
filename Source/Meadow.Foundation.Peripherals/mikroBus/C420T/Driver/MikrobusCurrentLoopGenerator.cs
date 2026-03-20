using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.mikroBUS.Sensors;

/// <summary>
/// Implementation of current loop generator using the Mikrobus C420T module
/// </summary>
public class MikrobusCurrentLoopGenerator : ICurrentLoopGenerator
{
    private readonly C420T _transmitter;
    private Current _lastCurrent;

    /// <summary>
    /// Initializes a new instance of the MikrobusCurrentLoopTransmitter class
    /// </summary>
    /// <param name="connector">The Mikrobus connector</param>
    public MikrobusCurrentLoopGenerator(MikroBusConnector connector)
        : this(connector.SpiBus, connector.Pins.CS)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MikrobusCurrentLoopTransmitter class
    /// </summary>
    /// <param name="spiBus">The SPI bus for communication</param>
    /// <param name="chipSelect">The chip select pin</param>
    public MikrobusCurrentLoopGenerator(ISpiBus spiBus, IPin chipSelect)
    {
        _transmitter = new C420T(spiBus, chipSelect);
        SetOutputCurrent(0.004.Amps()).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the last output current that was set
    /// </summary>
    /// <returns>The current that was last output by this sensor</returns>
    public Current GetOutputCurrent()
    {
        return _lastCurrent;
    }

    /// <summary>
    /// Sets the output current for the transmitter
    /// </summary>
    /// <param name="current">The current to output</param>
    /// <returns>A completed task</returns>
    public Task SetOutputCurrent(Current current)
    {
        _transmitter.GenerateOutput(current);
        _lastCurrent = current;
        return Task.CompletedTask;
    }

}
