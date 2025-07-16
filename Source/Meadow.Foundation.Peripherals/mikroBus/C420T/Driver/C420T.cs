using Meadow.Foundation.ICs.DAC;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.mikroBUS.Sensors;

/// <summary>
/// Represents a mikroBUS current sensing 4-20mA transmitter
/// </summary>
public class C420T : IDisposable
{
    /// <summary>
    /// Reference voltage (2.048V)
    /// </summary>
    public Voltage ReferenceVoltage { get; protected set; } = new Voltage(2.048, Voltage.UnitType.Volts);

    private readonly Mcp4921 mcp4921;
    private readonly IDigitalOutputPort? csPort;
    private readonly IAnalogOutputPort dac;
    private readonly bool portCreated;

    /// <summary>
    /// Gets a value indicating whether the object has been disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates a new C420T object using the SPI bus for output
    /// </summary>
    /// <param name="connector">The Mikrobus connector for the device</param>
    public C420T(MikroBusConnector connector)
        : this(connector.SpiBus, connector.Pins.CS)
    {
    }

    /// <summary>
    /// Creates a new C420T object using the SPI bus for output
    /// </summary>
    /// <param name="spiBus">The SPI bus</param>
    /// <param name="chipSelectPin">Chip select pin</param>
    public C420T(ISpiBus spiBus,
        IPin chipSelectPin)
    {
        csPort = chipSelectPin.CreateDigitalOutputPort();
        portCreated = true;
        mcp4921 = new Mcp4921(spiBus, csPort);
        dac = mcp4921.CreateAnalogOutputPort(mcp4921.Pins.ChannelA, Mcp492x.Gain.Gain1x, false);
    }

    /// <summary>
    /// Creates a new C420T object using the SPI bus for output
    /// </summary>
    /// <param name="spiBus">The SPI bus</param>
    /// <param name="chipSelectPort">Chip select port</param>
    public C420T(ISpiBus spiBus,
        IDigitalOutputPort chipSelectPort)
    {
        portCreated = false;
        mcp4921 = new Mcp4921(spiBus, chipSelectPort);
        dac = mcp4921.CreateAnalogOutputPort(mcp4921.Pins.ChannelA, Mcp492x.Gain.Gain1x, false);
    }

    /// <summary>
    /// Generates a 4-20mA output
    /// </summary>
    /// <param name="output">The desired output current</param>
    public Task GenerateOutput(Current output)
    {
        if (output.Milliamps < 4 || output.Milliamps > 20)
        {
            throw new ArgumentOutOfRangeException("Output must be in the range of 4-20 mA");
        }

        dac.GenerateOutput((uint)(output.Milliamps * 200));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                if (portCreated)
                {
                    csPort?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
