using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using System.Threading;
using Meadow.Units;
using System;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Ws2812 neoPixels;

        private readonly int ledCount = 24;

        public override Task Initialize()
        {
            var spiBus = Device.CreateSpiBus();
            neoPixels = new Ws2812(spiBus, ledCount);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                for (int i = 0; i < neoPixels.NumberOfLeds; i++)
                {
                    neoPixels.SetAllLeds(Color.Black);
                    neoPixels.SetLed(i, Color.Purple);
                    neoPixels.Show();
                    Thread.Sleep(100);
                }
            }
        }
    }
}