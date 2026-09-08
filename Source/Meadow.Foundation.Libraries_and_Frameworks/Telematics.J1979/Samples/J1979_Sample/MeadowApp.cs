using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace Obd2.ClientSample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");


            return Task.CompletedTask;
        }

        public override async Task Run()
        {

            await Task.Delay(1000);

        }
    }
}