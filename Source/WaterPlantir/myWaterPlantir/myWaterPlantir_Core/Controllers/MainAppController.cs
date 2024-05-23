/* ****************** The control process occurs here *****************************
 * The process is best described as a sequential batch reactor that passes through 5 control stages. During the
 * 1) WAITING stage,effluent is discharged into a receiving tank. When the receiving tank is full,or
 * if a set time (MAX_WAIT_TIL_FILL) has elapsed and the fill level has reached a minimum level,the process progresses to the
 * 2) FILLING stage where the liquor from the receiving tank is intermittently pumped into the reactor tank. The time
 * estmated to fill the reaction tank, FILL_TIME, is dependent on BILGE1ONTIME, the time in milliseconds that the pump runs
 * on every BILGE1_ON_NTH control cycle. The control cycle runs with an interval of UPDATE_INTERVAL_MIN minutes.
 * when the receiving tank is empty or the FILL_TIME has elapsed, the process progress to the
 * 3) AERATION stage where aeration maintains dissolved O2 between set values, OX_LOW_O2_MG_L and OX_HIGH_O2_MG_L and 
 * the reactor tank is kept heated to within LOW_TEMP_DEG and HIGH_TEMP_DEG degrees C. 
 * After the AERATION_TIME_HR has elapsed, the system progresses to the
 * 4) SETTLING stage, where temperature is maintaines as before but dissolved oxygen levels are reduced to between
 * OX_LOW_O2_MG_L and OX_HIGH_O2_MG_L. After SETTLING_TIME_HR has elapsed, the process progresses to the final
 * 5) EMPTYING stage where bilgePump2 empties the contents of the reaction tank by turning on continuously
 * for EMPTY_TIME_MIN minutes. After emptying stage, process stage is reset to waiting stage
 * 
 * Q: QUESTION: We empty reaction tank as soon as possible (SETTLING_TIME_HR), but would it make sense to
 * empty it only when the receiving tank is full and we are ready to start a new batch?
 * ********************************************************************************/

#define TESTING     // if TESTING is defined, we use different (shortened) process time spans
//#undef TESTING

using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WaterPlantir.Hardware;
using WaterPlantir.Models;

namespace WaterPlantir.Controllers
{
    public class MainAppController
    {
        public event EventHandler<WaterPlantirConditionsModel> ConditionsUpdated = default!;

        protected IWaterPlantirHardware Hardware { get; set; }
        protected DisplayController displayController;
        protected bool IsRunning = false;
        protected WaterPlantirConditionsModel WaterPlantirConditions { get; set; }

        int TIMEZONE_OFFSET = -8; // UTC-8

        IWiFiNetworkAdapter Network;

        //==== variables for stage of control process
        protected int control_stage = 0;                  // current stage of the control process, see below
        protected const int STARTING = 0;            // sets hardware to default state, inits timer for waiting to fill stage
        protected const int WAITING = 1;             // Waits for full receiving tank or waits for max duration
        protected const int FILLING = 2;             // fills reaction tank from receiving tank, intermittently pulsing bilgePump1
        protected const int AERATION = 3;            // maintains temp and high O2 levels in reaction tank, maybe stir pump?
        protected const int SETTLING = 4;            // maintains temp and low O2 levels in reaction tank, no stirring
        protected const int EMPTYING = 5;            // empties the reaction tank by running bilgepump2 continuously
        protected const int SLUDGE = 6;                 // runs sludge pump

        //==== Modify these constants to change process control variables
        protected const double UPDATE_INTERVAL_MIN = 0.5;       // how often process control function runs, in minutes

        protected const double RECEIVE_TANK_FULL_CM = 15;       // distance sensor value when receive tank is full, in cm
        protected const double RECEIVE_TANK_EMPTY_CM = 57;      // distance sensor value when receive tank is empty, in cm
        protected const double RECEIVE_TANK_RUN_MAX_CM = 30;    // maximum distance value for a which a batch will be started

        protected const UInt32 BILGE1_ON_NTH = 4;               // when filling, pump turns on only on every BILGE1_ON_NTH time process runs
        protected const double BILGE1_ON_TIME_SEC = 3;          // when filling, pump turns on for this many seconds when it runs

        protected const double LOW_TEMP_DEG = 15;               // Low temp turn on for heater, degrees C
        protected const double HIGH_TEMP_DEG = 25;              // high temperature turn off for heater, degrees C


        protected const double ANOX_LOW_O2_MG_L = 0.2;          // Low oxygen level turn of aerator in Settling stage, mg/L
        protected const double ANOX_HIGH_O2_MG_L = 0.4;         // high level turn off of aerator in Settling stage, mg/L

        protected const double SLUDGE_TIME_MIN = 0.5;
        protected const UInt32 SLUDGE_ON_NTH = 4;               // sludge empties every nth cycle


#if TESTING                                                     // can define a different (i.e., shorter) set of time spans for use in testing 
        protected const double MAX_WAIT_TIL_FILL_HR = 0.6;
        protected const double FILL_TIME_HR = 0.1;
        protected const double AERATION_TIME_HR = 0.1;
        protected const double SETTLING_TIME_HR = 0.1;
        protected const double EMPTY_TIME_MIN = 1;
        protected const double OX_LOW_O2_MG_L = 10;            // Low oxygen level turn of aerator in Aeration stage, mg/L
        protected const double OX_HIGH_O2_MG_L = 15;             // high level turn off of aerator in Aeration stage, mg/L
        
#else
        protected const double MAX_WAIT_TIL_FILL_HR = 6.0;      // Maximum waiting time, after this time avance to fill stage if max level is met
        protected const double FILL_TIME_HR = 2.5;              // duration for the filling stage, may need to change this if pump timing is changed
        protected const double AERATION_TIME_HR = 2.0;          // duration of aeation stage, in hours
        protected const double SETTLING_TIME_HR = 1.5;          // duration of settling stage, in hours
        protected const double EMPTY_TIME_MIN = 5;              // duration of emptying stage, in minutes

        protected const double OX_LOW_O2_MG_L = 3.5;            // Low oxygen level turn of aerator in Aeration stage, mg/L
        protected const double OX_HIGH_O2_MG_L = 4;             // high level turn off of aerator in Aeration stage, mg/L
#endif
        //==== timespans, temperatures, lengths, etc. will be initialized in constructor with values from constants
        protected TimeSpan UPDATE_INTERVAL, MAX_WAIT_TIL_FILL, FILL_TIME, AERATION_TIME, SETTLING_TIME, EMPTY_TIME, SLUDGE_TIME, ELAPSED_TIME;
        protected Temperature LOW_TEMP, HIGH_TEMP;
        protected ConcentrationInWater OX_LOW_O2, OX_HIGH_O2, ANOX_LOW_O2, ANOX_HIGH_O2;
        protected Length RECEIVE_TANK_FULL, RECEIVE_TANK_EMPTY, RECEIVE_TANK_RUN_MIN;
        protected System.Timers.Timer bilge1Timer;
        protected DateTime startTime, endTime;                     // for start and end time for stages of process
        UInt64 ControlCounter;                                     // used to track BILGE1_ON_NTH control cycle for turning on pump
        UInt64 CycleCounter;

        //==== Constructor for MainAppController
        public MainAppController(IWaterPlantirHardware hardware, bool isSimulator = false)
        {
            //==== init hardware, WaterPLantir sensors and relays

            Hardware = hardware;

            CycleCounter = 0;
            //==== init time intervals from constants
            UPDATE_INTERVAL = TimeSpan.FromMinutes(UPDATE_INTERVAL_MIN);
            MAX_WAIT_TIL_FILL = TimeSpan.FromHours(MAX_WAIT_TIL_FILL_HR);
            FILL_TIME = TimeSpan.FromHours(FILL_TIME_HR);   // time span for filling receiving tank, from pre-set value
            AERATION_TIME = TimeSpan.FromHours(AERATION_TIME_HR);
            SETTLING_TIME = TimeSpan.FromHours(SETTLING_TIME_HR);
            EMPTY_TIME = TimeSpan.FromMinutes(EMPTY_TIME_MIN);
            EMPTY_TIME = TimeSpan.FromMinutes(SLUDGE_TIME_MIN);
            //==== init temperatures from constants
            LOW_TEMP = new Temperature(LOW_TEMP_DEG, Temperature.UnitType.Celsius);
            HIGH_TEMP = new Temperature(HIGH_TEMP_DEG, Temperature.UnitType.Celsius);
            //==== init concentrations from constants
            OX_LOW_O2 = new ConcentrationInWater(OX_LOW_O2_MG_L, ConcentrationInWater.UnitType.MilligramsPerLiter);
            OX_HIGH_O2 = new ConcentrationInWater(OX_HIGH_O2_MG_L, ConcentrationInWater.UnitType.MilligramsPerLiter);
            ANOX_LOW_O2 = new ConcentrationInWater(ANOX_LOW_O2_MG_L, ConcentrationInWater.UnitType.MilligramsPerLiter);
            ANOX_HIGH_O2 = new ConcentrationInWater(ANOX_HIGH_O2_MG_L, ConcentrationInWater.UnitType.MilligramsPerLiter);
            //==== init receiving tank distances fom constants
            RECEIVE_TANK_EMPTY = new Length(RECEIVE_TANK_EMPTY_CM, Length.UnitType.Centimeters);
            RECEIVE_TANK_RUN_MIN = new Length(RECEIVE_TANK_RUN_MAX_CM, Length.UnitType.Centimeters);
            RECEIVE_TANK_FULL = new Length(RECEIVE_TANK_FULL_CM, Length.UnitType.Centimeters);
            //==== create timer for bilge pump
            bilge1Timer = new System.Timers.Timer(BILGE1_ON_TIME_SEC * 1000)
            {
                AutoReset = false,
                Enabled = false,
            };
            bilge1Timer.Elapsed += stopBilge1;
            //==== set control stage to starting
            control_stage = STARTING;
          
            //==== LED to RED
            Hardware.RgbLed?.SetColor(Color.Red);
            //==== Display Controller
            if (Hardware.Display is { } display)
            {
                displayController = new DisplayController(display, isSimulator ? RotationType.Normal : RotationType._270Degrees);
            }
        }


        public Task Run()
        {
            
            _ = UpdateTask(UPDATE_INTERVAL);

            return Task.CompletedTask;
        }

        /*
        protected void SubscribeToCloudConnectionEvents()
        {

            Resolver.UpdateService.StateChanged += (sender, state) =>
            {
                //displayController?.UpdateStatus(state.ToString());
            };
        }
        */

        private void A02yyuw_DistanceUpdated(object sender, IChangeResult<Length> e)
        {
            if (Math.Abs (e.New.Centimeters - WaterPlantirConditions.DistanceToTopOfLiquid.Value.Centimeters) > 1)
            {
                Resolver.Log.Info($"Distance Changed: {e.New.Centimeters:N1} cm");
                WaterPlantirConditions.DistanceToTopOfLiquid = e.New;
                displayController.UpdateModel(WaterPlantirConditions);
            }
           
        }

        private void Thermistor_TempreatureUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
            if (Math.Abs (e.New.Celsius - WaterPlantirConditions.ThermistorOneTemp.Value.Celsius) > 1)
            {
                Resolver.Log.Info($"Temperature Changed: {e.New.Celsius:N1} °C");
                WaterPlantirConditions.ThermistorOneTemp = e.New;
                displayController.UpdateModel(WaterPlantirConditions);
            }
        }


        private void DOsensor_ConcentrationUpdated(object sender, IChangeResult<ConcentrationInWater> e)
        {
            if (Math.Abs(e.New.MilligramsPerLiter - e.Old.Value.MilligramsPerLiter) > 0.2)
            {
                Resolver.Log.Info($"Ox Conc Changed: {e.New.MilligramsPerLiter:N1} mg/l");
                WaterPlantirConditions.Concentration = e.New;
                displayController.UpdateModel(WaterPlantirConditions);
            }
        }
            protected async Task UpdateTask(TimeSpan updateInterval)
        {

            if (IsRunning)
            {
                return;
            }
            IsRunning = true;

            // do a one-off read of all the sensors and log 
            WaterPlantirConditions = await ReadSensors();
            Resolver.Log.Info($"Thermistor 1: {WaterPlantirConditions.ThermistorOneTemp?.Celsius:N2}°C");
            Resolver.Log.Info($"Distance to Top of Liquid: {WaterPlantirConditions.DistanceToTopOfLiquid?.Centimeters:N2}cm");
            Resolver.Log.Info($"Concentration: {WaterPlantirConditions.Concentration?.MilligramsPerLiter}mg/l");
            displayController.UpdateModel(WaterPlantirConditions);

            Hardware.DistanceSensor.Updated += A02yyuw_DistanceUpdated;

            Hardware.Thermistor_One.Updated += Thermistor_TempreatureUpdated;

            Hardware.DissolvedOxygenMeter.Updated += DOsensor_ConcentrationUpdated;

            Hardware.Thermistor_One.StartUpdating(TimeSpan.FromSeconds (5));
            Hardware.DissolvedOxygenMeter.StartUpdating(TimeSpan.FromSeconds(5));
            Hardware.DistanceSensor.StartUpdating(TimeSpan.FromSeconds (2));


            while (IsRunning)
            {

                GC.Collect();

                //TODO: Move these to the new SensorService Pattern to reduce tasks.

              
                //Resolver.Log.Info($"Temperature: {WaterPlantirConditions.Temperature?.Celsius:N2}°C | Humidity: {WaterPlantirConditions.Humidity?.Percent:N2}%");
                Resolver.Log.Info($"Thermistor 1: {WaterPlantirConditions.ThermistorOneTemp?.Celsius:N1}°C");
                Resolver.Log.Info($"Distance to Top of Liquid: {WaterPlantirConditions.DistanceToTopOfLiquid?.Centimeters:N1}cm");
                Resolver.Log.Info($"Concentration: {WaterPlantirConditions.Concentration?.MilligramsPerLiter:N1}mg/l");
                displayController.UpdateModel(WaterPlantirConditions);

                // call our process update
                ControlProcessUpdate(Hardware, WaterPlantirConditions);

            

                    await Task.Delay(updateInterval);
                
            }
        }

        protected void StopUpdating()
        {
            if (!IsRunning)
            {
                return;
            }
            Hardware.Thermistor_One.StopUpdating();
            Hardware.DissolvedOxygenMeter.StopUpdating();
            Hardware.DistanceSensor.StopUpdating();
            IsRunning = false;
        }

        public void SetWiFiStatus(bool connected)
        {
            //displayController.UpdateWifi(connected);
        }
        

        /// <summary>
        /// Performs a one-off read of all the sensors.
        /// </summary>
        /// <returns></returns>
        protected async Task<WaterPlantirConditionsModel> ReadSensors()
        {

            Resolver.Log.Info($"Reading sensors.");

            TimeSpan timeoutDuration = TimeSpan.FromSeconds(5);

            var temperatureTask = Hardware.TemperatureSensor?.Read();
            var humidityTask = Hardware.HumiditySensor?.Read();
            var pressureTask = Hardware.PressureSensor?.Read();
            var thermistorOneTask = Hardware.Thermistor_One?.Read();
            var distanceSensorTask = Hardware.DistanceSensor?.Read();
            var dissolvedOxygenTask = Hardware.DissolvedOxygenMeter.Read();

            // run the tasks in serial with timeouts
            await Task.WhenAny(temperatureTask, Task.Delay(timeoutDuration));
            await Task.WhenAny(humidityTask, Task.Delay(timeoutDuration));
            await Task.WhenAny(pressureTask, Task.Delay(timeoutDuration));
            await Task.WhenAny(thermistorOneTask, Task.Delay(timeoutDuration));
            await Task.WhenAny(distanceSensorTask, Task.Delay(timeoutDuration));
            await Task.WhenAny(dissolvedOxygenTask, Task.Delay(timeoutDuration));

            Resolver.Log.Info($"Sensor reads completed.");

            // pull the data out and put into the model
            var climate = new WaterPlantirConditionsModel()
            {
                Temperature = temperatureTask.IsCompletedSuccessfully ? temperatureTask?.Result : null,
                Humidity = (humidityTask.IsCompletedSuccessfully ? humidityTask?.Result : null),
                Pressure = (pressureTask.IsCompletedSuccessfully ? pressureTask?.Result : null),
                ThermistorOneTemp = (thermistorOneTask.IsCompletedSuccessfully ? thermistorOneTask?.Result : null),
                DistanceToTopOfLiquid = (distanceSensorTask.IsCompletedSuccessfully ? distanceSensorTask?.Result : null),
                Concentration = (dissolvedOxygenTask.IsCompletedSuccessfully ? dissolvedOxygenTask?.Result : null),

            };

            ConditionsUpdated?.Invoke(this, climate);

            return climate;
        }

        protected void ControlProcessUpdate(IWaterPlantirHardware Hardware, WaterPlantirConditionsModel WaterPlantirConditions)
        {
            Resolver.Log.Info("Doing control process stuff.");

            switch (control_stage)
            {
                case STARTING:     //  starting up, sets MAX_WAIT timer and turns off all relays

                    Hardware.Relay110_Sump.IsOpen = true;
                    Hardware.Relay110_Aerator.IsOpen = true;
                    Hardware.Relay110_Heater.IsOpen = true;
                    Hardware.Relay110_Extra.IsOpen = true;
                    Hardware.Relay12_Bilge1.IsOpen = true;
                    Hardware.Relay12_Bilge2.IsOpen = true;
                    Hardware.Relay12_Stir.IsOpen = true;
                    Hardware.Relay12_Extra.IsOpen = true;

                    startTime = DateTime.Now;
                    endTime = startTime.Add(MAX_WAIT_TIL_FILL);
                    control_stage = WAITING;
                    displayController?.UpdateStatus("START:" + startTime.ToString());
                    ControlCounter = 0;
                    break;

                case WAITING:
                    if ((WaterPlantirConditions.DistanceToTopOfLiquid < RECEIVE_TANK_FULL) || ((DateTime.Compare(DateTime.Now, endTime) > 0) && WaterPlantirConditions.DistanceToTopOfLiquid < RECEIVE_TANK_RUN_MIN))
                    {
                        control_stage = FILLING;
                        startTime = DateTime.Now;
                        endTime = startTime.Add(FILL_TIME);
                        //Hardware.Relay110_Aerator.IsClosed = true;
                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    displayController?.UpdateStatus("WAIT: " + ELAPSED_TIME.Hours.ToString("D2") + ":" + ELAPSED_TIME.Minutes.ToString("D2") + ":" + ELAPSED_TIME.Seconds.ToString("D2"));

                    break;

                case FILLING:
                    if ((WaterPlantirConditions.DistanceToTopOfLiquid > RECEIVE_TANK_EMPTY) || (DateTime.Compare(DateTime.Now, endTime) > 0))
                    {
                        control_stage = AERATION;
                        startTime = DateTime.Now;
                        endTime = startTime.Add(AERATION_TIME);
                        Hardware.Relay110_Aerator.IsClosed = true;
                    }
                    else
                    {
                       
                        
                        if ((WaterPlantirConditions.ThermistorOneTemp < LOW_TEMP) && (Hardware.Relay110_Heater.IsOpen))
                        {
                            Hardware.Relay110_Heater.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.ThermistorOneTemp > HIGH_TEMP) && Hardware.Relay110_Heater.IsClosed)
                            {
                                Hardware.Relay110_Heater.IsOpen = true;
                            }
                        }
                        
                        if ((WaterPlantirConditions.Concentration < OX_LOW_O2) && Hardware.Relay110_Aerator.IsOpen)
                        {
                            Hardware.Relay110_Aerator.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.Concentration > OX_HIGH_O2) && Hardware.Relay110_Aerator.IsClosed)
                            {
                                Hardware.Relay110_Aerator.IsOpen = true;
                            }
                        }
                        
                        if (ControlCounter % BILGE1_ON_NTH == 0)    // turn on pump every Nth time through
                        {
                            Hardware.Relay12_Bilge1.IsClosed = true;
                            bilge1Timer.Enabled = true;
                        }
                        ControlCounter += 1;
                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    var remaningTime = FILL_TIME - ELAPSED_TIME;
                    displayController?.UpdateStatus("FILL: " + remaningTime.Hours.ToString("D2") + ":" + remaningTime.Minutes.ToString("D2") + ":" + remaningTime.Seconds.ToString("D2"));
                    break;


                case AERATION:
                    if (DateTime.Compare(DateTime.Now, endTime) > 0)
                    {
                        control_stage = SETTLING;
                        startTime = DateTime.Now;
                        endTime = startTime.Add(SETTLING_TIME);
                        Hardware.Relay110_Aerator.IsOpen = true;
                    }
                    else
                    {
                        if ((WaterPlantirConditions.ThermistorOneTemp < LOW_TEMP) && (Hardware.Relay110_Heater.IsOpen))
                        {
                            Hardware.Relay110_Heater.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.ThermistorOneTemp > HIGH_TEMP) && Hardware.Relay110_Heater.IsClosed)
                            {
                                Hardware.Relay110_Heater.IsOpen = true;
                            }
                        }
                        
                        if ((WaterPlantirConditions.Concentration < OX_LOW_O2) && Hardware.Relay110_Aerator.IsOpen)
                        {
                            Hardware.Relay110_Aerator.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.Concentration > OX_HIGH_O2) && Hardware.Relay110_Aerator.IsClosed)
                            {
                                Hardware.Relay110_Aerator.IsOpen = true;
                            }
                        }
                        
                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    remaningTime = AERATION_TIME - ELAPSED_TIME;
                    displayController?.UpdateStatus("AER: " + remaningTime.Hours.ToString("D2") + ":" + remaningTime.Minutes.ToString("D2") + ":" + remaningTime.Seconds.ToString("D2"));

                    break;

                case SETTLING:
                    if (DateTime.Compare(DateTime.Now, endTime) > 0)
                    {
                        Hardware.Relay110_Heater.IsOpen = true;
                        Hardware.Relay110_Aerator.IsOpen = true;
                        control_stage = EMPTYING;
                        startTime = DateTime.Now;
                        endTime = startTime.Add(EMPTY_TIME);
                        Hardware.Relay12_Bilge2.IsClosed = true;
                    }
                    else
                    {
                        if ((WaterPlantirConditions.ThermistorOneTemp < LOW_TEMP) && (Hardware.Relay110_Heater.IsOpen))
                        {
                            Hardware.Relay110_Heater.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.ThermistorOneTemp > HIGH_TEMP) && Hardware.Relay110_Heater.IsClosed)
                            {
                                Hardware.Relay110_Heater.IsOpen = true;
                            }
                        }
                        
                        if ((WaterPlantirConditions.Concentration < ANOX_LOW_O2) && Hardware.Relay110_Aerator.IsOpen)
                        {
                            Hardware.Relay110_Aerator.IsClosed = true;
                        }
                        else
                        {
                            if ((WaterPlantirConditions.Concentration > ANOX_HIGH_O2) && Hardware.Relay110_Aerator.IsClosed)
                            {
                                Hardware.Relay110_Aerator.IsOpen = true;
                            }
                        }
                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    remaningTime = SETTLING_TIME - ELAPSED_TIME;
                    displayController?.UpdateStatus("SETTL: " + remaningTime.Hours.ToString("D2") + ":" + remaningTime.Minutes.ToString("D2") + ":" + remaningTime.Seconds.ToString("D2"));
                    break;

                case EMPTYING:

                    if (DateTime.Compare(DateTime.Now, endTime) > 0)
                    {
                        Hardware.Relay12_Bilge2.IsOpen = true;
                        CycleCounter += 1;
                        if (CycleCounter % SLUDGE_ON_NTH == 0)
                        {
                            control_stage = SLUDGE;
                            startTime = DateTime.Now;
                            endTime = startTime.Add(SLUDGE_TIME);
                            Hardware.Relay110_Sump.IsClosed = true;
                        }
                        else
                        {
                            control_stage = STARTING;

                        }

                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    displayController?.UpdateStatus("EMPT: " + ELAPSED_TIME.ToString());
                    break;


                case SLUDGE:
                    if (DateTime.Compare(DateTime.Now, endTime) > 0)
                    {
                        Hardware.Relay110_Sump.IsOpen = true;
                        control_stage = STARTING;

                    }
                    ELAPSED_TIME = DateTime.Now.Subtract(startTime);
                    displayController?.UpdateStatus("SLUDG: " + ELAPSED_TIME.ToString());
                    break;
            }

            var cloudLogger = Resolver.Services.Get<CloudLogger>();
            cloudLogger?.LogEvent(1000, "Sensor reading", new Dictionary<string, object>()
                    {

                        { "Temperature", $"{WaterPlantirConditions.Temperature?.Celsius:N1}" },
                        { "Thermistor 1", $"{WaterPlantirConditions.ThermistorOneTemp?.Celsius:N1}"},
                        { "Distance To Top", $"{WaterPlantirConditions.DistanceToTopOfLiquid?.Centimeters:N1}"},

                        { "Concentration", $"{WaterPlantirConditions.Concentration?.MilligramsPerLiter:N1}"},
                        { "Control Stage", $"{control_stage}"},
                    });
        }

        private void stopBilge1(Object source, ElapsedEventArgs e)
        {
            Hardware.Relay12_Bilge1.IsOpen = true;
        }
    }
}

