using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.HT16K33_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ht16k33 ht16k33;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");
            ht16k33 = new Ht16k33(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override async Task Run()
        {
            int index = 0;
            bool on = true;

            while (true)
            {
                ht16k33.SetLed((byte)index, on);
                ht16k33.UpdateDisplay();
                index++;

                if (index >= 128)
                {
                    index = 0;
                    on = !on;
                }

                await Task.Delay(100);
            }
        }

        //<!=SNOP=>
    }
}