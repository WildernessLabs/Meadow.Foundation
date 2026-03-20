using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated voltage sensor
/// </summary>
public class SimulatedVoltageSensor : SimulatedSamplingSensorBase<Voltage>, IVoltageSensor
{
    private readonly Voltage maxVoltage;
    private readonly Voltage minVoltage;

    /// <inheritdoc/>
    public override Type ValueType => typeof(Voltage);

    /// <inheritdoc/>
    public Voltage? Voltage { get; private set; }

    /// <summary>
    /// Creates a SimulatedCurrentSensor instance
    /// </summary>
    public SimulatedVoltageSensor(Voltage? maxVoltage = null, Voltage? minVoltage = null)
    {
        this.minVoltage = minVoltage ?? Units.Voltage.Zero;
        this.maxVoltage = maxVoltage ?? 120.Volts();

        Voltage = 0.Volts();
    }

    /// <inheritdoc/>
    public ValueTask<Voltage> ReadVoltage()
    {
        return new ValueTask<Voltage>(Voltage ?? Units.Voltage.Zero);
    }

    /// <inheritdoc/>
    public override void SetSensorValue(object value)
    {
        Voltage = (Voltage)value;
    }

    /// <inheritdoc/>
    protected override Voltage GenerateSimulatedValue(SimulationBehavior behavior)
    {
        switch (behavior)
        {
            case SimulationBehavior.RandomWalk:
                var r = GetRandomDouble(minVoltage.Volts, maxVoltage.Volts);
                Voltage = new Units.Voltage(r);
                break;
        }

        return Voltage!.Value;
    }
}
