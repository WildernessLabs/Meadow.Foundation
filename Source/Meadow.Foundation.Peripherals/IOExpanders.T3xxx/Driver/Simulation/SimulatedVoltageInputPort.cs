using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a simulated voltage input port on a Temco Controls T3 module.
/// </summary>
public class SimulatedVoltageInputPort : IVoltageInputPort
{
    private readonly IT3Module _module;
    private readonly Random _random;

    /// <inheritdoc/>
    public IPin Pin { get; }

    internal SimulatedVoltageInputPort(IT3Module module, IPin pin)
    {
        _module = module;
        Pin = pin;
        _random = new Random();
    }

    /// <inheritdoc/>
    public ValueTask<Voltage> Read()
    {
        return new ValueTask<Voltage>(
            new Voltage(_random.NextDouble() * 10d, Voltage.UnitType.Volts));
    }
}
