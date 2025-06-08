using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Driver for thermistor temperature sensors
/// </summary>
/// <remarks>
/// Typical wiring (high-side)
/// 
/// 3.3V >-----[ 10k R ]---+-------------&lt; Analog_in
///                        |
///                        +---[ TM ]--- &lt; GND
/// </remarks>
public class Thermistor : PollingSensorBase<Units.Temperature>, ISamplingTemperatureSensor
{/// <summary>
 /// The analog input port for the thermistor
 /// </summary>
    protected IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// The type of thermistor (NTC or PTC)
    /// </summary>
    public ThermistorType Type { get; }

    /// <summary>
    /// The placement of the thermistor in the circuit (high-side or low-side)
    /// </summary>
    public ThermistorPlacement Placement { get; }

    /// <summary>
    /// The reference voltage used in the circuit
    /// </summary>
    public Voltage ReferenceVoltage { get; }

    /// <summary>
    /// The reference resistor value used in the voltage divider
    /// </summary>
    public Resistance ReferenceResistor { get; }

    /// <summary>
    /// Beta value for NTC thermistors (B parameter equation)
    /// </summary>
    public double BetaValue { get; set; }

    /// <summary>
    /// Resistance at reference temperature (typically 25°C)
    /// </summary>
    public Resistance ReferenceResistance { get; set; }

    /// <summary>
    /// Reference temperature for the thermistor calibration
    /// </summary>
    public Units.Temperature ReferenceTemperature { get; set; }

    /// <summary>
    /// Calibration offset to adjust temperature readings
    /// </summary>
    public Units.Temperature CalibrationOffset { get; set; } = new Units.Temperature(0, Units.Temperature.UnitType.Celsius);

    /// <summary>
    /// The last read temperature value
    /// </summary>
    public Units.Temperature? Temperature => GetTemperature();

    /// <summary>
    /// Creates a new Thermistor object
    /// </summary>
    /// <param name="analogInputPort">The analog input the thermistor is connected to</param>
    /// <param name="referenceResistor">The value of the reference resistor in the voltage divider</param>
    /// <param name="type">The type of thermistor (NTC or PTC)</param>
    /// <param name="placement">The placement of the thermistor in the circuit (high-side or low-side)</param>
    /// <param name="referenceVoltage">The reference voltage for the circuit</param>
    /// <param name="referenceResistance">The resistance at reference temperature</param>
    /// <param name="referenceTemperature">The reference temperature (typically 25°C)</param>
    /// <param name="betaValue">The beta value for NTC thermistors</param>
    /// <param name="updateInterval">The time between reads</param>
    public Thermistor(
        IAnalogInputPort analogInputPort,
        Resistance referenceResistor,
        ThermistorType type = ThermistorType.NTC,
        ThermistorPlacement placement = ThermistorPlacement.LowSide,
        Voltage? referenceVoltage = null,
        Resistance? referenceResistance = null,
        Units.Temperature? referenceTemperature = null,
        double betaValue = 3950,
        TimeSpan? updateInterval = null)
    {
        ReferenceResistor = referenceResistor;
        Type = type;
        Placement = placement;
        ReferenceVoltage = referenceVoltage ?? new Voltage(3.3, Voltage.UnitType.Volts);
        ReferenceResistance = referenceResistance ?? new Resistance(10, Resistance.UnitType.Kiloohms);
        ReferenceTemperature = referenceTemperature ?? new Units.Temperature(25, Units.Temperature.UnitType.Celsius);
        BetaValue = betaValue;

        // Create analog input port
        AnalogInputPort = analogInputPort;

        // Configure update interval
        base.UpdateInterval = updateInterval ?? TimeSpan.FromSeconds(1);
        base.StartUpdating();
    }

    /// <summary>
    /// Reads the current temperature from the thermistor
    /// </summary>
    /// <returns>The current temperature</returns>
    protected override async Task<Units.Temperature> ReadSensor()
    {
        // Read the analog value
        var voltage = await AnalogInputPort.Read();

        // Calculate resistance based on the placement of the thermistor
        Resistance thermistorResistance;

        if (Placement == ThermistorPlacement.LowSide)
        {
            // Low-side placement formula
            thermistorResistance = new Resistance(
                ReferenceResistance.Ohms * voltage.Volts / (ReferenceVoltage.Volts - voltage.Volts),
                Resistance.UnitType.Ohms);
        }
        else // High-side placement
        {
            // High-side placement formula
            thermistorResistance = new Resistance(
                ReferenceResistor.Ohms * (ReferenceVoltage.Volts - voltage.Volts) / voltage.Volts,
                Resistance.UnitType.Ohms);
        }

        // Adjust the formula based on thermistor type
        if (Type == ThermistorType.NTC)
        {
            // NTC calculation using the Beta parameter equation
            double steinhart = Math.Log(thermistorResistance.Ohms / ReferenceResistance.Ohms) / BetaValue;
            steinhart += 1.0 / (ReferenceTemperature.Celsius + 273.15);
            var tempKelvin = 1.0 / steinhart;

            // Convert from Kelvin to Celsius and apply calibration offset
            var temperature = new Units.Temperature(tempKelvin - 273.15, Units.Temperature.UnitType.Celsius);
            return temperature + CalibrationOffset;
        }
        else // PTC
        {
            // For PTC, we use a simpler linear approximation
            // This is a simplified approach and might need refinement for specific PTC models
            double tempRatio = thermistorResistance.Ohms / ReferenceResistance.Ohms;
            var tempChange = new Units.Temperature((tempRatio - 1) * 100, Units.Temperature.UnitType.Celsius);
            return ReferenceTemperature + tempChange + CalibrationOffset;
        }
    }

    /// <summary>
    /// Gets the current temperature
    /// </summary>
    /// <returns>The current temperature</returns>
    public Units.Temperature GetTemperature()
    {
        return ReadSensor().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Sets a calibration offset to adjust temperature readings
    /// </summary>
    /// <param name="actualTemperature">The known actual temperature for calibration</param>
    public async Task CalibrateAsync(Units.Temperature actualTemperature)
    {
        var measuredTemp = await ReadSensor();
        // Calculate the offset needed to match the actual temperature
        CalibrationOffset = actualTemperature - measuredTemp;
    }

    /// <summary>
    /// Convenience method for setting the calibration offset directly
    /// </summary>
    /// <param name="offset">The temperature offset to apply</param>
    public void SetCalibrationOffset(Units.Temperature offset)
    {
        CalibrationOffset = offset;
    }
}