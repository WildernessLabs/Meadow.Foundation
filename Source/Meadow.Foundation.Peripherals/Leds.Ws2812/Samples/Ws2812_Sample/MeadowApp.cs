using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Units;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Ws2812 _ws2812;

        private readonly int ledCount = 10;

        public override Task Initialize()
        {
            var _spiBus = Device.CreateSpiBus(new Frequency(3.2, Frequency.UnitType.Megahertz));
            _ws2812 = new Ws2812(_spiBus, ledCount);

            return base.Initialize();
        }

        public override Task Run()
        {
            for (var i = 0; i < ledCount; i++)
            {
                if (i % 2 == 0)
                {
                    _ws2812.SetLed(i, Color.Blue);
                }
                else
                {
                    _ws2812.SetLed(i, Color.Red);
                }
                _ws2812.Show();
            }
            return Task.CompletedTask;
        }
    }
}