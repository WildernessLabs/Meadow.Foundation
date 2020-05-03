using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        SerialLcd display;

        public MeadowApp()
        {
            Initialize();

            Console.WriteLine("Clear");
            display.ClearLines();

            Console.WriteLine("Set brightness");
            display.SetBrightness(0.5f);

            Console.WriteLine("WriteLine");
            display.WriteLine("Hello LCD", 0);

        //    Console.WriteLine("Toggle splash");
        //    display.ToggleSplashScreen();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            display = new SerialLcd(Device, Device.SerialPortNames.Com4);
        }
    }
}