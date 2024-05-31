using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation;
using System;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using DOAnalogTempCorr;

namespace MeadowApp
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        IProjectLabHardware projectLab;
        SteinhartHartCalculatedThermistor tempSensor;
        AnalogDO_Temp DOSensor;
        protected FilterableChangeObserver<Meadow.Units.Temperature> tempConsumer;
        protected FilterableChangeObserver<ConcentrationInWater> DOconsumer;
        DateTime startTime;


        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            IAnalogInputPort DOport = projectLab.IOTerminal.Pins.A1.CreateAnalogInputPort(20);
            tempSensor = new SteinhartHartCalculatedThermistor(
                projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10),
                new Resistance(10, Resistance.UnitType.Kiloohms));
            DOSensor = new AnalogDO_Temp(DOport, new Temperature(0,Temperature.UnitType.Celsius));
            tempConsumer = SteinhartHartCalculatedThermistor.CreateObserver(
               handler: result =>
               {
                   DOSensor.WaterTemperature = result.New;
                   //Resolver.Log.Info($"Water Temp: {result.New.Celsius:N2}°C");
               },
               filter: result =>
               {
                   if (result.Old is { } old)
                   {
                       return (result.New - old).Abs().Celsius > 0.1; // returns true if > 0.1°C change.
                   }
                   return false;
               }
            );
            tempSensor.Subscribe(tempConsumer);


            DOconsumer = AnalogDO_Temp.CreateObserver(
                handler: result =>
                {
                    //Resolver.Log.Info($"0xygen Conc:{result.New.MilligramsPerLiter:N1} mg/l");
                    // Print data in three columns so it can be copied and pasted into IgorPro or other package
                    TimeSpan elapsed = DateTime.Now - startTime;
                    Resolver.Log.Info($"{result.New.MilligramsPerLiter:N1} {DOSensor.WaterTemperature.Celsius:N1} {elapsed}");

                },
                //filter: null
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Abs().MilligramsPerLiter >= 0.1; // returns true if change >= 0.1 mg/L.
                    }
                    return false;
                }
            );
            DOSensor.Subscribe(DOconsumer);
            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");
            //ReadSensor();
            startTime = DateTime.Now;
            tempSensor.StartUpdating(TimeSpan.FromSeconds(2));
            DOSensor.StartUpdating(TimeSpan.FromSeconds(2));
            return base.Run();
        }

        public void ReadSensor()
        {
            Resolver.Log.Info("Reading Sensors...");
            _ = tempSensor.Read();
            DOSensor.WaterTemperature = (Temperature)tempSensor.Temperature;
            Resolver.Log.Info($"Inital Water Temp: {DOSensor.WaterTemperature.Celsius:N1}°C");
            _ = DOSensor.Read();
            Resolver.Log.Info($"Initial Oxygen concentration: {DOSensor.Concentration.MilligramsPerLiter:N1}mg/l");
        }
    }
}
