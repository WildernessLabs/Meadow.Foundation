using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Sound;
using System.Threading.Tasks;

namespace Sensors.Sound.Ky038_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ky038 sensor;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Ky038(Device.Pins.A00, Device.Pins.D10);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}