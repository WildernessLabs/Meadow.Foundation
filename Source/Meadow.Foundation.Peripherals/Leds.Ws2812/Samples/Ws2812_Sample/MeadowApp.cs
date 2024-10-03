using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Ws2812 _ws2812;

        private readonly int ledCount = 20;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize");
            var _spiBus = Device.CreateSpiBus(new Frequency(3, Frequency.UnitType.Megahertz));
            _ws2812 = new Ws2812(_spiBus, ledCount);

            return base.Initialize();
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            var rand = new Random();

            var colors = new Color[]
                {
                    Color.Red,
                    Color.Orange,
                    Color.Yellow,
                    Color.Green,
                    Color.Blue,
                    Color.Violet,
                };

            var c = 0;

            while (true)
            {
                var color = colors[c];

                Resolver.Log.Info($"Change to {color.ToString()}");

                _ws2812.SetLed(9, Color.Black);
                _ws2812.SetLed(8, Color.Black);
                _ws2812.SetLed(7, Color.Black);
                _ws2812.SetLed(6, Color.Black);
                _ws2812.SetLed(5, Color.Black);
                _ws2812.SetLed(4, Color.Black);
                _ws2812.SetLed(3, Color.Black);
                _ws2812.SetLed(2, Color.Black);
                _ws2812.SetLed(1, Color.Black);
                _ws2812.SetLed(0, color);
                _ws2812.Show();
                await Task.Delay(1000);

                _ws2812.SetLed(9, Color.Black);
                _ws2812.SetLed(8, Color.Black);
                _ws2812.SetLed(7, Color.Black);
                _ws2812.SetLed(6, Color.Black);
                _ws2812.SetLed(5, Color.Black);
                _ws2812.SetLed(4, Color.Black);
                _ws2812.SetLed(3, Color.Black);
                _ws2812.SetLed(2, Color.Black);
                _ws2812.SetLed(1, Color.Black);
                _ws2812.SetLed(0, Color.Black);
                _ws2812.Show();

                c++;
                if (c > 5) c = 0;

                await Task.Delay(1000);
            }
        }
    }
}