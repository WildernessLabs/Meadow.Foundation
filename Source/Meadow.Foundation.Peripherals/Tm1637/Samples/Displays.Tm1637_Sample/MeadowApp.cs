using System;
using System.Threading;
using Meadow.Foundation.Displays.Tm1637;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Tm1637 display;

        public MeadowApp()
        {
            ConfigurePorts();

            var chars = new Character[] { Character.A, Character.B, Character.C, Character.D };

            display.Show(chars);

            /*
            Console.WriteLine("0");
            display.Display(0, Character.Digit0);
            Console.WriteLine("1");
            display.Display(1, Character.Digit1);
            Console.WriteLine("B");
            display.Display(2, Character.B);
            Console.WriteLine("A");
            display.Display(3, Character.A); */
        }

        public void ConfigurePorts()
        {
            Console.WriteLine("Creating output ports...");

            display = new Tm1637(Device, Device.Pins.D02, Device.Pins.D01);

            Console.WriteLine("Set brightness");
            display.Brightness = 7;

            Console.WriteLine("Turn screen on");
            display.ScreenOn = true;

            Console.WriteLine("Clear display");
            display.Clear();
        }
    }
}