using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a simulated current input port on a Temco Controls T3 module.
/// </summary>
public class SimulatedCurrentInputPort : ICurrentInputPort
{
    private readonly IT3Module _module;
    private readonly Random _random;

    /// <inheritdoc/>
    public IPin Pin { get; }

    internal SimulatedCurrentInputPort(IT3Module module, IPin pin)
    {
        _module = module;
        Pin = pin;
        _random = new Random();
    }

    /// <inheritdoc/>
    public ValueTask<Current> Read()
    {
        return new ValueTask<Current>(
            new Current(
            (_random.NextDouble() * 16d) + 4d,
             Current.UnitType.Milliamps));
    }
}
