using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Radio;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Audio.Radio.Tea5767_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Tea5767 radio;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");
            
            radio = new Tea5767(Device.CreateI2cBus());

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            //scan through avaliable stations
            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(1000);

                radio.SearchNextSilent();

                Console.WriteLine($"Current frequency: {radio.GetFrequency()}");
            }

            //set a known station
            radio.SelectFrequency(new Frequency(94.5, Frequency.UnitType.Megahertz));

            return base.Run();
        }

        //<!=SNOP=>
    }
}