using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.DigiPots;

/// <summary>
/// Represents a Mcp4xxx digital potentimeter or rheostat
/// </summary>
public abstract partial class Mcp4xxx : ISpiPeripheral
{
    protected ISpiCommunications SpiComms { get; }

    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;
    public Frequency DefaultSpiBusSpeed => new Frequency(10, Frequency.UnitType.Megahertz);

    public Resistor[] Resistors { get; }

    public Resistance MaxResistance { get; }
    public int MaxSteps { get; }

    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => SpiComms.BusMode;
        set => SpiComms.BusMode = value;
    }

    public Frequency SpiBusSpeed
    {
        get => SpiComms.BusSpeed;
        set => SpiComms.BusSpeed = value;
    }

    public Mcp4xxx(ISpiBus spiBus, IDigitalOutputPort chipSelect, int resistorCount, Resistance maxResistance, int resolutionSteps)
    {
        if (resistorCount < 0 && resistorCount > 1) throw new ArgumentException();

        MaxResistance = maxResistance;
        MaxSteps = resolutionSteps;

        SpiComms = new SpiCommunications(spiBus, chipSelect, DefaultSpiBusSpeed, DefaultSpiBusMode);
        Resistors = new Resistor[resistorCount];
        for (int i = 0; i < resistorCount; i++)
        {
            Resistors[i] = new Resistor(this, i, SpiComms, maxResistance, resolutionSteps);
        }
    }
}
