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
using Meadow.Foundation;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using Meadow.Logging;
using Meadow.Foundation.Sensors.Temperature;

using TempCorrectedDOSensorContract;
using Meadow;

namespace DFRobotGravityDOMeter;

/// <summary>
/// DFRobot Analog Gravity Dissolved Oxygen Meter
/// </summary>
public class DFRobotGravityDOMeter : PollingSensorBase<ConcentrationInWater>, ITempCorrectedDOsensor
{
    /// <summary>
    /// Last concentration value read from the sensor
    /// </summary>
    public ConcentrationInWater Concentration { get; set; } = new ConcentrationInWater(10, ConcentrationInWater.UnitType.MilligramsPerLiter);

    /// <summary>
    /// Last Temperature read from the sensor
    /// </summary>
    public Meadow.Units.Temperature WaterTemperature { get; set; } = new Temperature(20, Temperature.UnitType.Celsius);

    //public ISamplingSensor<Temperature>? TempSensor { get; set; } = null;
    /// <summary>
    /// A Temperature Sensor is nice, but not essential
    /// </summary>
    public ITemperatureSensor? TempSensor { get; set; } = null;

    /// <summary>
    /// The analog input port sampling the Oxygen sensor probe voltage
    /// </summary>
    private IAnalogInputPort DOsensorPort { get; }

    /// <summary>
    ///  constructor for DO sensor
    /// </summary>
    /// <param name="doSensorPort">Analog input port that samples oxygen sensor</param>
    /// <param name="tempSensor">Temeprature sensor, may be null</param>
    public DFRobotGravityDOMeter(IAnalogInputPort doSensorPort, ITemperatureSensor? tempSensor)
    {
        TempSensor = tempSensor;
        if (TempSensor is not null)
            TempSensor.Updated += TempSensorHandler;
        DOsensorPort = doSensorPort;
        DOsensorPort.Subscribe(IAnalogInputPort.CreateObserver(handler: HandleAnalogUpdate, filter: FilterAnalogUpdate));
    }

     /// <summary>
    /// This filter prevents Temperature calculations for changes in sensor voltage less than 10 mV
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private bool FilterAnalogUpdate(IChangeResult<Voltage> result)
    {
        bool filter = false;
        if (result.Old is { } old) //if (result.Old is not null) new Voltage old = result.old.Value
        {
            filter = Math.Abs(result.New.Millivolts - old.Millivolts) > 10;
        }
        return filter;
    }

    /// <summary>
    /// Update function calculates new concentration when filter threshold voltage is met
    /// </summary>
    /// <param name="result">Voltage change result,old and new</param>
    private void HandleAnalogUpdate(IChangeResult<Voltage> result)
    {
        ConcentrationInWater concentration = VoltageToConcentration(result.New, WaterTemperature);
        ChangeResult<ConcentrationInWater> ConcentrationChangeResult = new()
        {
            Old = this.Concentration,
            New = concentration,
        };
        this.Concentration = concentration;
        base.RaiseEventsAndNotify(ConcentrationChangeResult);
    }

    /// <summary>
    /// Observer for temperature sensor, updates DOMeter Water Temperature with latest value
    /// </summary>
    /// <param name="sender">The temperature sensor object</param>
    /// <param name="e">Temperature change result, old and new</param>
    void TempSensorHandler(object sender, IChangeResult<Temperature> e)
    {
        this.WaterTemperature = e.New;
    }


    /// <summary>
    /// Reads the temperature and O2 sensors, updates values and returns the concentration
    /// </summary>
    /// <returns>new concentration value</returns>
    Task<ConcentrationInWater> ITempCorrectedDOsensor.Read()
    {
        return ReadSensors();
    }

   /// <summary>
   /// Task that reads the sensors and updates the temperature/concentration value
   /// </summary>
   /// <returns>new concentration value</returns>
   async Task<ConcentrationInWater> ReadSensors()
    {
        if (TempSensor is not null)
            this.WaterTemperature = await TempSensor.Read();
        Voltage voltage = await DOsensorPort.Read();
        this.Concentration = VoltageToConcentration(voltage, WaterTemperature);
        return Concentration;

    }

    /// <summary>
    /// task that just reads the O2 sensor
    /// </summary>
    /// <returns>new concentration value</returns>
    protected override async Task<ConcentrationInWater> ReadSensor()
    {
        Voltage voltage = await DOsensorPort.Read();
        this.Concentration = VoltageToConcentration(voltage, WaterTemperature);
        return Concentration;
    }

    /// <summary>
    /// The calibration values for the sensor, log linear fits of voltage vs temperature
    /// for no oxygen condition,and for (mg/l)/V for saturated O2 condition
    /// </summary>
    public double Zero_Offset { get; set; } = -3.5382; // or a big negative number like -1E6 if no zero cal is available
    public double Zero_Mult { get; set; } = -0.086818; // // or 0 if no zero cal is available
    public double Sat_Offset { get; set; } = 4.6834;
    public double Sat_Mult { get; set; } = -0.086818;


    /// <summary>
    /// Calculates O2 concentration from sensor voltage and temperature
    /// </summary>
    /// <param name="voltage"></param>
    /// <param name="temperature"></param>
    /// <returns></returns>
    ConcentrationInWater VoltageToConcentration(Voltage voltage, Meadow.Units.Temperature temperature)
    {
        double milligramsPerVolt = Math.Exp(Sat_Offset + Sat_Mult * temperature.Celsius);
        return new ConcentrationInWater((voltage.Volts * milligramsPerVolt), Meadow.Units.ConcentrationInWater.UnitType.MilligramsPerLiter);
    }

     /// <summary>
     /// Default update interval for updating sensor on start updating
     /// </summary>
    TimeSpan ISamplingSensor.UpdateInterval { get; } = TimeSpan.FromSeconds(2d);

    /// <summary>
    /// Returns truth the sensor has been started updating
    /// </summary>
    bool ISamplingSensor.IsSampling { get; }

   
    /// <summary>
    /// gets a new DO cocentration after updating temp with provided temp
    /// useful if running without an attached temp sensor
    /// </summary>
    /// <param name="temperature">temperature obtained from some other method</param>
    /// <returns>DO concentration calculated with latest sensor voltage and provided temp</returns>
    ConcentrationInWater ITempCorrectedDOsensor.GetConcentrationWithTemp(Temperature temperature)
    {
        this.WaterTemperature = temperature;
        Concentration = VoltageToConcentration(DOsensorPort.Voltage, temperature);
        return Concentration;
    }

   /// <summary>
   /// Starts both DO sensor and temp sensor updating at updateInterval
   /// </summary>
   /// <param name="updateInterval">Optional update interval</param>
    void ISamplingSensor.StartUpdating(TimeSpan? updateInterval)
    {
        lock (samplingLock)
        {
            if (IsSampling) { return; }
            IsSampling = true;
            DOsensorPort.StartUpdating(updateInterval);
            TempSensor?.StartUpdating(updateInterval);
        }

        base.StartUpdating(updateInterval);
    }

    /// <summary>
    /// Stops DO sensor and temp sensor from updating automatically
    /// </summary>
    void ISamplingSensor.StopUpdating()
        {
        lock (samplingLock)
        {
            if (!IsSampling) { return; }
            IsSampling = false;
            DOsensorPort.StopUpdating();
            TempSensor?.StopUpdating();
        }

        base.StopUpdating();
    }

    /// <summary>
    /// Variable used track steps in calibration of probe for temperature
    /// </summary>
    private byte calState;
    public const byte MODE_MEASURE = 0;
    public const byte MODE_CAL_ZERO = 1;
    public const byte MODE_CAL_SAT = 2;
   

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
    /// variables used for making linear fit to calibration data
    /// </summary>
    private double SumXi, SumYi, SumXiYi, SumSqXi, Nreads;

}



/*
   





    /// <summary>
    /// The current water temperature, which can be set from the TempSensor
    /// </summary>
    public Units.Temperature WaterTemperature { get; set; } = new Units.Temperature(20, Units.Temperature.UnitType.Celsius);
    private Units.Temperature lastWaterTemperature;

    public Units.Voltage SensorVoltage;
    /// <summary>
    /// a Temperature Sensor, used to update water temperature for calulating mg/l
    /// </summary>
   

  
    private double SumXi, SumYi, SumXiYi, SumSqXi, Nreads;


   

  
   

    /// <summary>
    /// observer for notifying that sensor has been sensed/votage has changed
    /// </summary>
    FilterableChangeObserver<Voltage> Observer;

    

   

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



   
  
}*/