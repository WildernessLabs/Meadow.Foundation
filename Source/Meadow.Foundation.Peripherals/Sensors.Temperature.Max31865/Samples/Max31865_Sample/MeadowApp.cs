using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Max31865_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Max6675 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Max31865(
                Device.CreateSpiBus(),
                Device.Pins.D00,
                KnownSensorType.PT100,
                Wires.ThreeWire,
                430,
                ConversionFilterMode.Filter60Hz);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                try
                {
                    var temp = await sensor.Read();
                    Resolver.Log.Info($"Temp: {temp.Fahrenheit:N1}F");
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error(ex.Message);
                }

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}