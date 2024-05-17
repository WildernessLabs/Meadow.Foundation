using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using Meadow.Cloud;
using Meadow.Peripherals.Sensors;

namespace Sensors.Temperature.Thermistor_Sample
{
    //public class MeadowApp : App<F7FeatherV2>
    public class MeadowApp : App<F7CoreComputeV2> // for projectLab
    {
        //<!=SNIP=>

        private SteinhartHartCalculatedThermistor thermistor;
        protected IProjectLabHardware projectLab;
        protected FilterableChangeObserver<Meadow.Units.Temperature> tempConsumer;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            IPin thePin = projectLab.GroveAnalog.Pins.D0;
            //IPin thePin = projectLab.GroveAnalog.GetPin("A00");
            IAnalogInputPort thePort = thePin.CreateAnalogInputPort(10, TimeSpan.FromMilliseconds(2), new Voltage(3.3));
            //thermistor = new SteinhartHartCalculatedThermistor(Device.CreateAnalogInputPort(Device.Pins.A00), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));
            thermistor = new SteinhartHartCalculatedThermistor(thePort, new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));
            tempConsumer = SteinhartHartCalculatedThermistor.CreateObserver(
             handler: result =>
             {
                Resolver.Log.Info($"Consumer Water Temp: {result.New.Celsius:N2}°C");
             },
            // only notify if the change is greater than 0.5°C
            filter: result =>
            {
                if (result.Old is { } old)
                {
                    return (result.New - old).Abs().Celsius > 0.1; // returns true if > 0.5°C change.
                }
                return false;
            }
            );
            thermistor.Subscribe(tempConsumer);

            /*
            thermistor.Updated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Resolver.Log.Info($"Temperature Updated: {e.New.Fahrenheit:N1}F/{e.New.Celsius:N1}C");
            };*/

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await thermistor.Read();
            Resolver.Log.Info($"Current temperature: {temp.Fahrenheit:N1}F/{temp.Celsius:N1}C");

            thermistor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}
