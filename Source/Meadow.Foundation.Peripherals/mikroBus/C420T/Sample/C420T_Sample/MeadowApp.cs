using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors;
using System;
using System.Threading.Tasks;

namespace C420T_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private C420T transmitter;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing ...");

            transmitter = new C420T(Device.CreateSpiBus(), Device.Pins.D00);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var ma = 4;
            var direction = 1;

            while (true)
            {
                ma += direction;
                if (ma == 20)
                {
                    direction = -1;
                }
                else if (ma == 4)
                {
                    direction = 1;
                }

                var val = new Meadow.Units.Current(ma, Meadow.Units.Current.UnitType.Milliamps);

                Resolver.Log.Info($"Writing: {val.Milliamps:0.00} mA");
                transmitter?.GenerateOutput(val);

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}
