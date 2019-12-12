using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max7219 display;

        public MeadowApp()
        {
            Init();

        }

        public void Init()
        {
            Console.WriteLine("Init...");

            display = new Max7219(Device, Device.CreateSpiBus(), Device.Pins.D00, 2);
        }

    }
}