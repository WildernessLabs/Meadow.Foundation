/*  
 *  /************************* Dissolved Oxygen Meter with Temperature Correction *************************
 * Designed to work with the Dissolved Oxygen Meter from Atlas Scientific or as resold by DFRObts. But will
 * work with with any galvanic dissolved oxygen probe that ouputs analog voltage.
 * Although Atlas recommends using the setup only to calculate percent saturation of oxygen relative to the
 * partial pressure of oxygen in the atmosphere, we will do some temperature correction calulations
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
    public const byte MODE_MEASURE = 0;
    public const byte MODE_CAL_ZERO = 1;
    public const byte MODE_CAL_SAT = 2;
    private byte calState;

    /// <summary>
    /// The current water temperature, which can be set from the TempSensor
    /// </summary>
    public Units.Temperature WaterTemperature { get; set; } = new Units.Temperature(20, Units.Temperature.UnitType.Celsius);
    private Units.Temperature lastWaterTemperature;

    public Units.Voltage SensorVoltage;
    /// <summary>
    /// a Temperature Sensor, used to update water temperature for calulating mg/l
    /// </summary>
    readonly ITemperatureSensor TempSensor;

    /// <summary>
    /// The calibration values for the sensor, log linear fits of voltage vs temperature
    /// for no oxygen condition,and for (mg/l)/V for saturated O2 condition
    /// </summary>
    public double Zero_Offset { get; set; } = -3.5382; // or a big negative number like -1E6 if no zero cal is available
    public double Zero_Mult { get; set; } = -0.086818; // // or 0 if no zero cal is available
    public double Sat_Offset { get; set; } = 4.6834;
    public double Sat_Mult { get; set; } = -0.086818;

    private double SumXi, SumYi, SumXiYi, SumSqXi, Nreads;


    /// <summary>
    /// the analog input port  used for A/D conversion of amplified electrode potential
    /// </summary>
    public IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// Last concentration value read from the sensor
    /// </summary>
    public ConcentrationInWater? Concentration => Conditions;

    /// <summary>
    /// Constants for 3rd order poly for DO saturation (mg/L) with °C temperature,
    /// at 1 atm pressure for fresh water. Obtained from the literature
    /// good from 0 to 30 °C
    /// satuaration conc = DO_Sat_K0 + (DO_Sat_K1 * °C) + (DO_Sat_K1 * °C^2)
    /// </summary>
    readonly double DO_Sat_K0 = 14.502;
    readonly double DO_Sat_K1 = -0.35984;
    readonly double DO_Sat_K2 = 0.0043703;

    /// <summary>
    /// observer for notifying that sensor has been sensed/votage has changed
    /// </summary>
    FilterableChangeObserver<Voltage> Observer;

    /// <summary>
    /// Creates a new DFRobotGravityDOMeter object with an analog input port and a temperature sensor
    /// </summary>
    /// <param name="analogInputPort">The port for the analog input pin</param>
    /// <param name="tempSensor">Some method that can continuously sample temperatue</param>
    public DFRobotGravityDOMeter(IAnalogInputPort analogInputPort, ITemperatureSensor tempSensor)
    {
        TempSensor = tempSensor;
        AnalogInputPort = analogInputPort;
        calState = MODE_MEASURE;
        Observer = AddListener();
    }

    /// <summary>
    /// Reads new data from the sensor
    /// </summary>
    /// <returns>The new reading</returns>
    protected override async Task<ConcentrationInWater> ReadSensor()
    {
        SensorVoltage = await AnalogInputPort.Read();
        Conditions = VoltageToConcentration(SensorVoltage, WaterTemperature);
        return Conditions;
    }

    public void NoZeroCal()
    {
        Zero_Offset = 1E10;
        Zero_Mult = 0;

    }


    public void StartCalZero(TimeSpan updateInterval)
    {
        if (calState == MODE_MEASURE)
        {
            SumXi = 0;
            SumYi = 0;
            SumXiYi = 0;
            SumSqXi = 0;
            Nreads = 0;
            lastWaterTemperature = WaterTemperature;
            Updated += HandleCalZeroResult;
            calState = MODE_CAL_ZERO;
            StartUpdating(updateInterval);
            TempSensor.StartUpdating(updateInterval);
        }
    }

    public void EndCalZero()
    {

        if (calState == MODE_CAL_ZERO)
        {
            StopUpdating();
            TempSensor.StopUpdating();
            Zero_Mult = ((Nreads * SumXiYi) - (SumXi * SumYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
            Zero_Offset = ((SumYi * SumSqXi) - (SumXi * SumXiYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
            Updated -= HandleCalZeroResult;
            calState = 0;
        }
    }


    public void StartCalSat(TimeSpan updateInterval)
    {
        if (calState == MODE_MEASURE)
        {
            SumXi = 0;
            SumYi = 0;
            SumXiYi = 0;
            SumSqXi = 0;
            Nreads = 0;
            lastWaterTemperature = WaterTemperature;
            Updated += HandleCalSatResult;
            calState = MODE_CAL_SAT;
            StartUpdating(updateInterval);
            TempSensor.StartUpdating(updateInterval);
        }
    }



    public void EndCalSat()
    {

        if (calState == MODE_CAL_SAT)
        {
            StopUpdating();
            TempSensor.StopUpdating();
            Sat_Mult = ((Nreads * SumXiYi) - (SumXi * SumYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
            Sat_Offset = ((SumYi * SumSqXi) - (SumXi * SumXiYi)) / ((Nreads * SumSqXi) - (SumXi * SumXi));
            Updated -= HandleCalSatResult;
            calState = 0;
        }
    }



    /// <summary>
    /// Starts continuously sampling the DO sensor. The tempSensor is not set
    /// to StartUpdating. It is responsibility of code that provided the
    /// tempsensor to do that. For instance, it may  be updating the temp
    /// sensor at at its own preferred interval
    /// </summary>
    public override void StartUpdating(TimeSpan? updateInterval)
    {
        lock (samplingLock)
        {
            if (IsSampling) { return; }
            IsSampling = true;
            AnalogInputPort.StartUpdating(updateInterval);
            TempSensor.StartUpdating(updateInterval);
        }
    }

    /// <summary>
    /// Stops sampling the sensor. Again, we dont stop temp sensor here
    /// </summary>
    public override void StopUpdating()
    {
        lock (samplingLock)
        {
            if (!IsSampling) { return; }
            IsSampling = false;
            AnalogInputPort.StopUpdating();
            TempSensor.StopUpdating();
        }
    }


    // A filterable observer, with no filter
    protected FilterableChangeObserver<Voltage> AddListener()
    {
        _ = AnalogInputPort.Subscribe
        (
            Observer = IAnalogInputPort.CreateObserver
            (
                handler : result =>
                {
                    SensorVoltage = result.New;
                    WaterTemperature = (TempSensor.Temperature.HasValue) ? TempSensor.Temperature.Value : new Units.Temperature(20, Units.Temperature.UnitType.Celsius);

                    /* The next line can be used to report raw voltage, for testing */
                    Resolver.Log.Info($"DO Sensor Voltage: {result.New.Millivolts:N1} mV");
                    Resolver.Log.Info($"DO TempSensor temp: {WaterTemperature.Celsius:N1} °C");
                   
                    ChangeResult <ConcentrationInWater> changeResult = new ChangeResult<ConcentrationInWater>()
                    {
                        New = VoltageToConcentration(result.New, WaterTemperature),
                        Old = Concentration,
                    };
                    Conditions = changeResult.New;
                    RaiseEventsAndNotify(changeResult);
                },
                 filter: null
            )
        );
        return Observer;
    }


    void  HandleCalZeroResult(object sender, IChangeResult<ConcentrationInWater> e)
    {

            if (Math.Abs(lastWaterTemperature.Celsius - WaterTemperature.Celsius) > 0.5)
            {
                // get the natuaral log of the sensor voltage
                double ln_V = Math.Log(SensorVoltage.Volts);
                // add to the running sums of the needed values for the log linear fit
                SumXi += WaterTemperature.Celsius;
                SumSqXi += (WaterTemperature.Celsius * WaterTemperature.Celsius);
                SumXiYi = WaterTemperature.Celsius * ln_V;
                SumYi = ln_V;
                Nreads += 1;
                lastWaterTemperature = WaterTemperature;
            }
    }



    void HandleCalSatResult(object sender, IChangeResult<ConcentrationInWater> e)
    {

            if (Math.Abs(lastWaterTemperature.Celsius - WaterTemperature.Celsius) > 0.5)
            {
                // get the sensor voltage offset, i.e., the zero voltage at this temperature
                Voltage zeroVoltage = new Voltage(Math.Exp(Zero_Offset + WaterTemperature.Celsius * Zero_Mult), Voltage.UnitType.Volts);
                // get the saturated oxgen concentration value at this temperature from 3deg poly fit
                double milligramsPerL = DO_Sat_K0 + (DO_Sat_K1 * WaterTemperature.Celsius) + (DO_Sat_K2 * WaterTemperature.Celsius * WaterTemperature.Celsius);
                // calculate (milligrams/L)/V from this sensor reading, and take the natural log
                double ln_mg_V = Math.Log(milligramsPerL / ((SensorVoltage - zeroVoltage).Volts));
                // add to the running sums of the needed values for the log linear fit
                SumXi += WaterTemperature.Celsius;
                SumSqXi += (WaterTemperature.Celsius * WaterTemperature.Celsius);
                SumXiYi = WaterTemperature.Celsius * ln_mg_V;
                SumYi = ln_mg_V;
                Nreads += 1;
                lastWaterTemperature = WaterTemperature;
            }
    }



    ConcentrationInWater VoltageToConcentration(Voltage voltage, Units.Temperature temperature)
    {
        double milligramsPerVolt = Math.Exp(Sat_Offset + Sat_Mult * temperature.Celsius);
        return new ConcentrationInWater((voltage.Volts * milligramsPerVolt), Units.ConcentrationInWater.UnitType.MilligramsPerLiter);
    }
}