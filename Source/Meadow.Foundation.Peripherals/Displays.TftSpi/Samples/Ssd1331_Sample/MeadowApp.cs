using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace Displays.Tft.Ssd1331_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            var config = new SpiClockConfiguration(new Meadow.Units.Frequency(12000, Meadow.Units.Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode0);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Resolver.Log.Info("Create display driver instance");

            var display = new St7796s
            (
                spiBus: spiBus,
                resetPin: Device.Pins.D00,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                width: 320, height: 480
            );

            graphics = new MicroGraphics(display);
            graphics.CurrentFont = new Font8x8();

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();

            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, false);
            graphics.DrawText(5, 5, "Meadow F7");

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}