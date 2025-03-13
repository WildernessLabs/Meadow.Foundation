using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Thermistor temperature sensor object using the B-Parameter or Beta equation to determine temperature
/// </summary>
public class BParameterThermistor : Thermistor
{
    // Constants for the thermistor
    private const double B = 3950;  // B-parameter (in Kelvin)
    private const double T0 = 298.15; // Reference temperature T0 (in Kelvin, which is 25°C)

    /// <inheritdoc/>
    public override Resistance NominalResistance { get; }

    /// <summary>
    /// Gets the resistance of the fixed-value series resistor in your voltage divider circuit
    /// </summary>
    public Resistance SeriesResistance { get; }

    /// <summary>
    /// Creates an instance of a BParameterThermistor using a 10k resistor and a 10k thermistor
    /// </summary>
    /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
    public BParameterThermistor(IObservableAnalogInputPort analogInput)
        : this(analogInput, 10_000.Ohms(), 10_000.Ohms())
    {
    }

    /// <summary>
    /// Creates an instance of a BParameterThermistor
    /// </summary>
    /// <param name="analogInput">The analog input reading the thermistor voltage divider output</param>
    /// <param name="nominalResistance">The nominal resistance of the thermistor (e.g. 10kOhm for a 10k thermistor)</param>
    /// <param name="seriesResistance">The resistance of the fixed-value series resistor in your voltage divider circuit</param>
    public BParameterThermistor(
        IObservableAnalogInputPort analogInput,
        Resistance nominalResistance,
        Resistance seriesResistance
        )
        : base(analogInput)
    {
        SeriesResistance = seriesResistance;
        NominalResistance = nominalResistance;
    }

    /// <inheritdoc/>
    protected override async Task<Units.Temperature> ReadSensor()
    {
        var voltageReading = await AnalogInputPort.Read();

        // ohms
        var measuredResistance = (SeriesResistance.Ohms * voltageReading.Volts) / (AnalogInputPort.ReferenceVoltage.Volts - voltageReading.Volts);

        // Calculate temperature in Kelvin using the B-parameter equation
        double temperatureK = 1 / ((1 / T0) + (1 / B) * Math.Log(measuredResistance / NominalResistance.Ohms));

        return new Units.Temperature(temperatureK, Units.Temperature.UnitType.Kelvin);
    }
}
