using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors;
using System;
using System.Threading.Tasks;

namespace C420R_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private C420R receiver;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing ...");

            receiver = new C420R(Device.CreateSpiBus(), Device.Pins.D00);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                var r = await receiver.Read();
                Resolver.Log.Info($"Reading: {r.Milliamps:0.00} mA");
                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}
