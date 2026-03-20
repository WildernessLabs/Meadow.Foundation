using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors;
using System;
using System.Threading.Tasks;

namespace CACCurrent_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private CACCurrent currentClick;
        private const bool useSpi = false;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing ...");

            if (useSpi)
            {
                currentClick = new CACCurrent(
                    Device.CreateSpiBus(),
                    Device.Pins.D14.CreateDigitalOutputPort());
            }
            else
            {
                currentClick = new CACCurrent(Device.Pins.A00.CreateAnalogInputPort(5));
            }

            currentClick.Updated += OnCurrentUpdated;
            currentClick.StartUpdating();

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            while (true)
            {
                var r = await currentClick.Read();
                Resolver.Log.Info($"Reading: {r.Amps:0.00} A");
                await Task.Delay(1000);
            }
        }

        private void OnCurrentUpdated(object sender, IChangeResult<Meadow.Units.Current> e)
        {
            Resolver.Log.Info($"Current changed from {(e.Old ?? new Meadow.Units.Current(0)).Amps}A to {e.New.Amps}A");
        }

        //<!=SNOP=>
    }
}