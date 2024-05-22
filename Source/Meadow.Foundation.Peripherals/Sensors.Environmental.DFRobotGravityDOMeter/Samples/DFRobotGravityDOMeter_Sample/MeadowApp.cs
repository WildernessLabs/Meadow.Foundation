using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;


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
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            tempSensor = new SteinhartHartCalculatedThermistor(
                projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10),
                new Resistance(10, Resistance.UnitType.Kiloohms));
            IAnalogInputPort DOport = projectLab.IOTerminal.Pins.A1.CreateAnalogInputPort(10, TimeSpan.FromMilliseconds(40), new Voltage(3.3));
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
                        return (result.New - old).Abs().Celsius > 0.1; // returns true if > 0.5°C change.
                    }
                    return false;
                }
            );
            tempSensor.Subscribe(tempConsumer);

             DOconsumer = DFRobotGravityDOMeter.CreateObserver(
                 handler: result =>
                {
                    Resolver.Log.Info($"0xygen Conc:{result.New.MilligramsPerLiter:N0}mg/l");
                    //string oldValue = (result.Old is { } old) ? $"{old.MilligramsPerLiter:n0}" : "n/a";
                    //string newValue = $"{result.New.MilligramsPerLiter:n0}";
                    //Resolver.Log.Info($"New: {newValue}mg/l, Old: {oldValue}mg/l");
                },
                //filter: null
            filter: result =>
            {
                if (result.Old is { } old)
                {
                    return (result.New - old).Abs().MilligramsPerLiter > 0.05; // returns true if > 0.1 mg/L.
                }
                return false;
            } 

            );
            DOsensor.Subscribe(DOconsumer);




            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
            await ReadSensor();
            DOsensor.StartUpdating(TimeSpan.FromSeconds(1));
            tempSensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadSensor()
        {
            DOsensor.WaterTemperature = await tempSensor.Read();
            var concentration = await DOsensor.Read();
            Resolver.Log.Info($"Water Temp: {DOsensor.WaterTemperature.Celsius:N2}°C");
            Resolver.Log.Info($"Oxygen concentration: {concentration.MilligramsPerLiter:N0}mg/l");
        }

        //<!=SNOP=>
    }
}