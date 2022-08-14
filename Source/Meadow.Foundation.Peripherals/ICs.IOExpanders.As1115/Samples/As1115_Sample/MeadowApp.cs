using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.As1115_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        As1115 as1115;
        MicroGraphics graphics;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");
            as1115 = new As1115(Device, Device.CreateI2cBus(), Device.Pins.D03);
            as1115.KeyScanPressStarted += KeyScanPressStarted;

            graphics = new MicroGraphics(as1115);

            return base.Initialize();
        }

        private void KeyScanPressStarted(object sender, KeyScanEventArgs e)
        {
            Console.WriteLine($"{e.Button} pressed");
        }

        public override async Task Run()
        {
            graphics.Clear();
            graphics.DrawLine(0, 0, 7, 7, true);
            graphics.DrawLine(0, 7, 7, 0, true);

            graphics.Show();
        }

        //<!=SNOP=>
    }
}