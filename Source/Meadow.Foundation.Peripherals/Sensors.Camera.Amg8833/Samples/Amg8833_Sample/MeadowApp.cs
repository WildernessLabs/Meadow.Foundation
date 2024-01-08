using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace Sensors.Cameras.Amg8833_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Amg8833 camera;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            camera = new Amg8833(i2cBus);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            while (true)
            {
                var pixels = camera.ReadPixels();

                //output the first 4 pixels
                Resolver.Log.Info($"{pixels[0].Celsius:F1}°C, {pixels[1].Celsius:F1}°C, {pixels[2].Celsius:F1}°C, {pixels[3].Celsius:F1}°C");
            }
        }

        //<!=SNOP=>
    }
}