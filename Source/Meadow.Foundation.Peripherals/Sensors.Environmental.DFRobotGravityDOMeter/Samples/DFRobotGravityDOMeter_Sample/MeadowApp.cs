using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Sensors.Environmental.DFRobotGravityDOMeter_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        IProjectLabHardware projectLab;
        DFRobotGravityDOMeter DOsensor;
        SteinhartHartCalculatedThermistor tempSensor;
        protected FilterableChangeObserver<Meadow.Units.Temperature> tempConsumer;
        protected FilterableChangeObserver<ConcentrationInWater> DOconsumer;
        DateTime startTime;
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            tempSensor = new SteinhartHartCalculatedThermistor(
                projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10),
                new Resistance(10, Resistance.UnitType.Kiloohms));
            // IAnalogInputPort DOport = projectLab.IOTerminal.Pins.A1.CreateAnalogInputPort(20);
            IAnalogInputPort DOport = projectLab.MikroBus2.Pins.AN.CreateAnalogInputPort(20);
            DOsensor = new DFRobotGravityDOMeter(DOport, tempSensor);

      

            tempConsumer = SteinhartHartCalculatedThermistor.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Water Temp: {result.New.Celsius:N2}°C");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Abs().Celsius > 0.5; // returns true if > 0.5°C change.
                    }
                    return false;
                }
            );
            //tempSensor.Subscribe(tempConsumer);

             DOconsumer = DFRobotGravityDOMeter.CreateObserver(
                 handler: result =>
                {
                    //Resolver.Log.Info($"0xygen Conc:{result.New.MilligramsPerLiter:N1} mg/l");
                    // Print data in three columns so it can be copied and pasted into IgorPro or other package
                    TimeSpan elapsed = DateTime.Now - startTime;
                    Resolver.Log.Info($"{result.New.MilligramsPerLiter:N1} {DOsensor.WaterTemperature.Celsius:N1} {elapsed}");

                },
                //filter: null
            filter: result =>
            {
                if (result.Old is { } old)
                {
                    return (result.New - old).Abs().MilligramsPerLiter> 0.1; // returns true if change > 0.1 mg/L.
                }
                return false;
            } 

            );
            DOsensor.Subscribe(DOconsumer);
            //DOsensor.Updated += handleResult;


            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
            await ReadSensor();
            startTime = DateTime.Now;
            DOsensor.StartUpdating(TimeSpan.FromSeconds(2));
            tempSensor.StartUpdating(TimeSpan.FromSeconds(2));
        }

        protected async Task ReadSensor()
        {
            DOsensor.WaterTemperature = await tempSensor.Read();
            var concentration = await DOsensor.Read();
            Resolver.Log.Info($"Water Temp: {DOsensor.WaterTemperature.Celsius:N1}°C");
            Resolver.Log.Info($"Oxygen concentration: {concentration.MilligramsPerLiter:N1}mg/l");
        }

        void handleResult(object sender, IChangeResult<ConcentrationInWater> e)
        {
            Resolver.Log.Info($"Result Handler:{e.New.MilligramsPerLiter:N2} mg/l");
        }

        //<!=SNOP=>
    }
}