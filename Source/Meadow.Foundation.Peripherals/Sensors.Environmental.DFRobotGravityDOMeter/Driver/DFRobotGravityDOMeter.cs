using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// DFRobot Analog Gravity Dissolved Oxygen Meter
/// </summary>
public partial class DFRobotGravityDOMeter : SamplingSensorBase<ConcentrationInWater>, IDissolvedOxygenConcentrationSensor
{
    /// <summary>
    /// The current water temperature (default 25C)
    /// </summary>
    public Units.Temperature WaterTemperature { get; set; } = new Units.Temperature(25, Units.Temperature.UnitType.Celsius);

    /// <summary>
    /// The calibration value for the sensor at 25C (default 1.6V)
    /// </summary>
    public Voltage CalibrationAt25C { get; set; } = new Voltage(1.6, Voltage.UnitType.Volts);

    /// <summary>
    /// Returns the analog input port
    /// </summary>
    protected IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// Last concentration value read from the sensor
    /// </summary>
    public ConcentrationInWater? Concentration { get; protected set; }

    /// <summary>
    /// The disolved oxygen lookup table for temperature values from 0 to 40 degrees C
    /// </summary>
    readonly int[] DO_Table = new int[41] {
    14460, 14220, 13820, 13440, 13090, 12740, 12420, 12110, 11810, 11530,
    11260, 11010, 10770, 10530, 10300, 10080, 9860, 9660, 9460, 9270,
    9080, 8900, 8730, 8570, 8410, 8250, 8110, 7960, 7820, 7690,
    7560, 7430, 7300, 7180, 7070, 6950, 6840, 6730, 6630, 6530, 6410};

    /// <summary>
    /// Creates a new DFRobotGravityDOMeter object
    /// </summary>
    /// <param name="analogInputPin">Analog pin the temperature sensor is connected to</param>
    /// <param name="sampleCount">How many samples to take during a given reading</param>
    /// <param name="sampleInterval">The time, to wait in between samples during a reading</param>
    public DFRobotGravityDOMeter(IPin analogInputPin, int sampleCount = 5, TimeSpan? sampleInterval = null)
        : this(analogInputPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), new Voltage(3.3)))
    { }

    /// <summary>
    /// Creates a new DFRobotGravityDOMeter object
    /// </summary>
    /// <param name="analogInputPort">The port for the analog input pin</param>
    public DFRobotGravityDOMeter(IAnalogInputPort analogInputPort)
    {
        AnalogInputPort = analogInputPort;

        AnalogInputPort.Subscribe
        (
            IAnalogInputPort.CreateObserver(
                result =>
                {
                    ChangeResult<ConcentrationInWater> changeResult = new()
                    {
                        New = VoltageToConcentration(result.New),
                        Old = Concentration
                    };
                    Concentration = changeResult.New;
                    RaiseEventsAndNotify(changeResult);
                }
            )
       );
    }

    /// <summary>
    /// Get the current voltage, useful for calibration
    /// </summary>
    /// <returns></returns>
    public Task<Voltage> GetCurrentVoltage()
    {
        return AnalogInputPort.Read();
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override async Task<ConcentrationInWater> ReadSensor()
    {
        var voltage = await AnalogInputPort.Read();
        var newConcentration = VoltageToConcentration(voltage);
        Concentration = newConcentration;
        return newConcentration;
    }

    /// <summary>
    /// Starts continuously sampling the sensor
    /// </summary>
    public override void StartUpdating(TimeSpan? updateInterval)
    {
        lock (samplingLock)
        {
            if (IsSampling) { return; }
            IsSampling = true;
            AnalogInputPort.StartUpdating(updateInterval);
        }
    }

    /// <summary>
    /// Stops sampling the sensor
    /// </summary>
    public override void StopUpdating()
    {
        lock (samplingLock)
        {
            if (!IsSampling) { return; }
            IsSampling = false;
            AnalogInputPort.StopUpdating();
        }
    }

    ConcentrationInWater VoltageToConcentration(Voltage voltage)
    {
        var calibrationValue = DO_Table[(int)WaterTemperature.Celsius];

        var voltageSaturationInMilliVolts = CalibrationAt25C.Millivolts + 35 * (WaterTemperature.Celsius - 25);
        var concentrationRaw = voltage.Millivolts * calibrationValue / voltageSaturationInMilliVolts;

        return new ConcentrationInWater(concentrationRaw, Units.ConcentrationInWater.UnitType.MicrogramsPerLiter);
    }
}