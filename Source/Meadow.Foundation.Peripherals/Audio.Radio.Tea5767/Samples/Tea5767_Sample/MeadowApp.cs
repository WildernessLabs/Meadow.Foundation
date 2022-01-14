using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Radio;

namespace Audio.Radio.Tea5767_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");
            
            var radio = new Tea5767(Device.CreateI2cBus());

            //scan through avaliable stations
            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(1000);

                radio.SearchNextSilent();

                Console.WriteLine($"Current frequency: {radio.GetFrequency()}");
            }

            //set a known station
            radio.SelectFrequency(new Meadow.Units.Frequency(94.5, Meadow.Units.Frequency.UnitType.Megahertz));
        }

        //<!—SNOP—>
    }
}