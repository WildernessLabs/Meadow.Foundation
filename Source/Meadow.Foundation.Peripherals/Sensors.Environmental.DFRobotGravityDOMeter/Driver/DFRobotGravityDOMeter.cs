/*  
 *  /****************************** Dissolved Oxygen Meter **********************************
 * Designed to work with the Dissolved Oxygen Meter from Atlas Scientific with a galvanic
 * dissolved oxygen probe. Although Atlas recommends using the setup only to calculate percent saturation
 * of oxygen relative to the partial pressure of oxygen in the atmosphere, we will do some calulations
 * and make some simplifications to estimate dissolved oxygen in mg/L. First among these is neglecting the effects 
 * of atmospheric pressure; instead, we assume pressure is close to standard. We also ignore salinity so
 * use only in fresh water.
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
public class DFRobotGravityDOMeter : SamplingSensorBase<ConcentrationInWater>, IDissolvedOxygenConcentrationSensor
{

    /// <summary>
    /// The current water temperature, which can be set from the TempSensor
    /// </summary>
    public Units.Temperature WaterTemperature { get; set; }

    public Units.Voltage SensorVoltage;
    /// <summary>
    /// a Temperature Sensor, used to keep temp up to date
    /// </summary>
    readonly ITemperatureSensor TempSensor;

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
    public IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// Last concentration value read from the sensor
    /// </summary>
    public ConcentrationInWater? Concentration => Conditions;

    /// <summary>
    /// Constants for 3rd order poly for DO saturation (mg/L) with temperature, good from 0 to 30 °C
    /// </summary>
    readonly double DO_Sat_K0 = 14.502;
    readonly double DO_Sat_K1 = -0.35984;
    readonly double DO_Sat_K2 = 0.0043703;

     /// <summary>
    /// observer for setting conditions
    /// </summary>
    FilterableChangeObserver<Voltage> Observer;

    /// <summary>
    /// Creates a new DFRobotGravityDOMeter object with an analog inpuyt port and a temperature sensor
    /// </summary>
    /// <param name="analogInputPort">The port for the analog input pin</param>
    public DFRobotGravityDOMeter(IAnalogInputPort analogInputPort, ITemperatureSensor tempSensor)
    {
        TempSensor = tempSensor;
        AnalogInputPort = analogInputPort;
        Observer = AddListener();
    }

    /// <summary>
    /// Reads new data from the sensor
    /// </summary>
    /// <returns>The new reading</returns>
    protected override async Task<ConcentrationInWater> ReadSensor()
    {
        SensorVoltage = await AnalogInputPort.Read();
        Conditions = VoltageToConcentration(SensorVoltage);
        return Conditions;
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

    protected FilterableChangeObserver<Voltage> AddListener()
    {
        _ = AnalogInputPort.Subscribe
        (
            Observer = IAnalogInputPort.CreateObserver
            (
                result =>
                {
                    SensorVoltage = result.New;
                    Resolver.Log.Info($"Voltage: {result.New.Millivolts:N1} mV");
                    ChangeResult<ConcentrationInWater> changeResult = new ChangeResult<ConcentrationInWater>()
                    {
                        New = VoltageToConcentration(result.New),
                        Old = Concentration,
                    };
                    Conditions = changeResult.New;
                    RaiseEventsAndNotify(changeResult);
                }
            )
        );
        //Updated += HandleResult;
        return Observer;
    }

    void HandleResult(object sender, IChangeResult<ConcentrationInWater> e)
    {
        //Resolver.Log.Info($"Result Handler:{Conditions.MilligramsPerLiter :N1} mg/l");
    }


    ConcentrationInWater VoltageToConcentration(Voltage voltage)
    {
        WaterTemperature = (TempSensor.Temperature.HasValue) ? (Units.Temperature)TempSensor.Temperature : new Units.Temperature (20, Units.Temperature.UnitType.Celsius);
        Voltage satVatTemp = new Voltage(sat_Offset + WaterTemperature.Celsius * sat_Mult, Voltage.UnitType.Volts);
        Voltage zeroVatTemp = new Voltage(zero_Offset + WaterTemperature.Celsius * zero_Mult, Voltage.UnitType.Volts);
        var propSat = (voltage.Volts - zeroVatTemp.Volts) / (satVatTemp.Volts - zeroVatTemp.Volts);
        var mg_per_ml_sat = DO_Sat_K0 + DO_Sat_K1 * WaterTemperature.Celsius + DO_Sat_K2 * WaterTemperature.Celsius * WaterTemperature.Celsius;

        return new ConcentrationInWater(propSat * mg_per_ml_sat, Units.ConcentrationInWater.UnitType.MilligramsPerLiter);
    }
}