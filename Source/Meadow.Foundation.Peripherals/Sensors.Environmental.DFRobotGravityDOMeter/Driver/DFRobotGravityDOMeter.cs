/*  
 */
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
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
    /// /// The current voltage from the A/D conversion of DO sensor
    /// </summary>
    public Voltage SensorVoltage { get; set; }

    /// <summary>
    /// The current water temperature, which can be read from the TempSensor
    /// </summary>
    protected Units.Temperature WaterTemperature;

    /// <summary>
    /// a Temperature Sensor, used to keep tempertaure up to date
    /// </summary>
    readonly ITemperatureSensor tempSensor;

    /// <summary>
    /// The calibration values for the sensor, linear fits of voltage vs temperature for saturated and no oxygen  
    /// </summary>
    public double sat_Offset { get; set; } = 0.12454;
    public double sat_Mult { get; set; } = 0.015984;
    public double zero_Offset { get; set; } = 0.020734;
    public double zero_Mult { get; set; } = 0.0019617;


    /// <summary>
    /// the analog input port  used for A/D conversion of amplified electrode potential
    /// </summary>
    protected IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// Last concentration value read from the sensor
    /// </summary>
    public ConcentrationInWater? Concentration { get; protected set; }

    /// <summary>
    /// Constants for 3rd order poly for DO saturation (mg/L) with temperature, good from 0 to 30 °C
    /// </summary>
    readonly double DO_Sat_K0 = 14.502;
    readonly double DO_Sat_K1 = -0.35984;
    readonly double DO_Sat_K2 = 0.0043703;

 
    /// <summary>
    /// Creates a new DFRobotGravityDOMeter object with an analog inpuyt port and a temperature sensor
    /// </summary>
    /// <param name="analogInputPort">The port for the analog input pin</param>
    public DFRobotGravityDOMeter(ITemperatureSensor TempSensor, IAnalogInputPort analogInputPort)
    {
        AnalogInputPort = analogInputPort;
        tempSensor = TempSensor;

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
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override async Task<ConcentrationInWater> ReadSensor()
    {
        SensorVoltage = await AnalogInputPort.Read();
        var newConcentration = VoltageToConcentration(SensorVoltage);
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
        WaterTemperature = (Units.Temperature)tempSensor.Temperature;
        Voltage satVatTemp = new Voltage(sat_Offset + WaterTemperature.Celsius * sat_Mult, Voltage.UnitType.Volts);
        Voltage zeroVatTemp = new Voltage(zero_Offset + WaterTemperature.Celsius * zero_Mult, Voltage.UnitType.Volts);
        var propSat = (voltage.Volts - zeroVatTemp.Volts) / (satVatTemp.Volts - zeroVatTemp.Volts);
        var mg_per_ml_sat = DO_Sat_K0 + DO_Sat_K1 * WaterTemperature.Celsius + DO_Sat_K2 * WaterTemperature.Celsius * WaterTemperature.Celsius;

        return new ConcentrationInWater(propSat * mg_per_ml_sat, Units.ConcentrationInWater.UnitType.MilligramsPerLiter);
    }
}