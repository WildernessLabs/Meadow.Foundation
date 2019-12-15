using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Radio;

namespace Audio.Radio.TEA5767_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected Tea5767 radio;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");
            
            radio = new Tea5767(Device.CreateI2cBus());

            TestTEA5767();
        }

        protected void TestTEA5767() 
        {
            Console.WriteLine("TestTEA5767...");

            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(1000);

                radio.SearchNextSilent();

                Console.WriteLine($"Current frequency: {radio.GetFrequency()}");
            }

            radio.SelectFrequency(94.5f);
        }
    }
}