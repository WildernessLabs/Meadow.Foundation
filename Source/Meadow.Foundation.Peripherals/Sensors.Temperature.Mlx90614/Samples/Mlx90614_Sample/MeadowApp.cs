using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System.Threading.Tasks;

namespace Sensors.Temperature.Mlx90614_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Mlx90614 sensor;

        public override async Task Run()
        {
            sensor = new Mlx90614(Device.CreateI2cBus(1));

            while (true)
            {
                var temp = await sensor.Read();
                Resolver.Log.Info($"Temperature: {temp.Celsius:N1}C ({temp.Fahrenheit:N1}F)");
                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}