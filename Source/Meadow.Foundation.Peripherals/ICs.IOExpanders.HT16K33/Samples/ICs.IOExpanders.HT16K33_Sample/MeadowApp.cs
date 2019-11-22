using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

namespace ICs.IOExpanders.HT16K33_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected HT16K33 ht16k33;

        public MeadowApp()
        {
            Console.WriteLine("Initialize...");
            ht16k33 = new HT16K33(Device.CreateI2cBus());

            TestHT16K33();
        }

        protected void TestHT16K33() 
        {
            Console.WriteLine("TestHT16K33...");

            int index = 0;
            bool on = true;

            while (true)
            {
                ht16k33.ToggleLed((byte)index, on);
                ht16k33.UpdateDisplay();
                index++;

                if (index >= 128)
                {
                    index = 0;
                    on = !on;
                }

                Thread.Sleep(100);
            }
        }
    }
}