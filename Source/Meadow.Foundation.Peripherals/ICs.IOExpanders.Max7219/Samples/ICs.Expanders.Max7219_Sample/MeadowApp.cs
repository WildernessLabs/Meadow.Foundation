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

            display.TestDisplay();

            for (int i = 0; i < 8; i++)
            {
                display[0, i] = 0xFF;
                display.Show();
                Thread.Sleep(50);
            }

            for(byte i = 0; i < 255; i++)
            {
                display[0, 0] = i;
                display[0, 1] = i;
                display[0, 2] = i;
                display[0, 3] = i;
                display[0, 4] = i;
                display[0, 5] = i;
                display[0, 6] = i;
                display[0, 7] = i;
                display.Show();
            }
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            display = new Max7219(Device, Device.CreateSpiBus(), Device.Pins.D02, 1);
        }

    }
}