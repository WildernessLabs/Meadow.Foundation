using System;
using System.Threading;
using Meadow.Foundation.Displays;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!—SNIP—>

        Tm1637 display;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            display = new Tm1637(Device, Device.Pins.D02, Device.Pins.D01);

            display.Brightness = 7;
            display.ScreenOn = true;

            display.Clear();

            var chars = new Character[] { Character.A, Character.B, Character.C, Character.D };

            display.Show(chars);
        }

        //<!—SNOP—>
    }
}