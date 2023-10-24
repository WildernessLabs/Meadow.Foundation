using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xxx digital potentimeter or rheostat
/// </summary>
public abstract partial class Mcp4xxx : ISpiPeripheral
{
    /// <summary>
    /// Gets the ISpiCommunications used to ineract with the SPI bus
    /// </summary>
    protected ISpiCommunications SpiComms { get; }

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;
    /// <inheritdoc/>
    public Frequency DefaultSpiBusSpeed => new Frequency(10, Frequency.UnitType.Megahertz);

    /// <summary>
    /// Gets the maximum resistance of the digital potentiometer.
    /// </summary>
    public Resistance MaxResistance { get; }
    /// <summary>
    /// Gets the maximum number of steps or resolution.
    /// </summary>
    public abstract int MaxSteps { get; }

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => SpiComms.BusMode;
        set => SpiComms.BusMode = value;
    }

    /// <inheritdoc/>
    public Frequency SpiBusSpeed
    {
        get => SpiComms.BusSpeed;
        set => SpiComms.BusSpeed = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mcp4xxx"/> class.
    /// </summary>
    /// <param name="spiBus">The SPI bus to use for communication.</param>
    /// <param name="chipSelect">The digital output port for chip select.</param>
    /// <param name="resistorCount">The resistor count.</param>
    /// <param name="maxResistance">The maximum resistance of the digital potentiometer.</param>
    public Mcp4xxx(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance)
    {
        if (resistorCount < 0 && resistorCount > 1) throw new ArgumentException();

        MaxResistance = maxResistance;

        SpiComms = new SpiCommunications(spiBus, chipSelect, DefaultSpiBusSpeed, DefaultSpiBusMode);
    }
}
