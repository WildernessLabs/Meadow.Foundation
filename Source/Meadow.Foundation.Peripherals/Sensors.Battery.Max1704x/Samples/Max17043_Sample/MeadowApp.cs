using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Battery;
using System.Threading.Tasks;

namespace Sensors.Battery.Max17043_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Max17043 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Max17043(Device.CreateI2cBus());

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                Resolver.Log.Info($"Voltage: {(await sensor.ReadVoltage()).Volts:N2}V ");
                Resolver.Log.Info($"SoC: {await sensor.ReadStateOfCharge():N0}% ");
                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}