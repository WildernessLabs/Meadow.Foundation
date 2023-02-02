using System;
using Meadow.Foundation.Displays;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Tm1637 display;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            display = new Tm1637(Device, Device.Pins.D02, Device.Pins.D01);

            display.Brightness = 7;
            display.ScreenOn = true;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            display.Clear();

            var chars = new Character[] { Character.A, Character.B, Character.C, Character.D };

            display.Show(chars);

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}