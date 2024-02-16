using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading.Tasks;

namespace Displays.ePaper.EpdColor_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        enum EpdColorDisplay
        {
            Epd1in54b,
            Epd1in54c,
            Epd2in13b,
            Epd2in13b_V4,
            Epd2in7b,
            Epd2in9b,
            Epd4in2bc,
            Epd4in2bV2,
        }

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize ...");

            var displayType = EpdColorDisplay.Epd4in2bV2;

            Resolver.Log.Info($"{displayType} selected - change displayType to select a different display");

            var display = GetDisplay(displayType);

            graphics = new MicroGraphics(display)
            {
                Rotation = RotationType._270Degrees
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run");

            for (int i = 0; i < 100; i++)
            {
                graphics.DrawPixel(i, i, Color.Black);
            }

            graphics.DrawRectangle(10, 40, 120, 60, Color.Black, true);
            graphics.DrawRectangle(20, 80, 120, 90, Color.Red, true);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(2, 20, "Meadow F7", Color.Black);
            graphics.DrawText(30, 50, "Color", Color.Red);
            graphics.DrawText(50, 90, "Black", Color.Black);
            graphics.DrawText(50, 120, "White", Color.White);

            graphics.Show();

            Resolver.Log.Info("Run complete");

            return Task.CompletedTask;
        }

        EPaperTriColorBase GetDisplay(EpdColorDisplay displayType)
        {
            //Initialize the display based on the displayType enum
            EPaperTriColorBase display = displayType switch
            {
                EpdColorDisplay.Epd1in54b => new Epd1in54b(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd1in54c => new Epd1in54c(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd2in13b => new Epd2in13b(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd2in13b_V4 => new Epd2in13b_V4(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd2in7b => new Epd2in7b(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd2in9b => new Epd2in9b(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd4in2bV2 => new Epd4in2bV2(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                EpdColorDisplay.Epd4in2bc => new Epd4in2bc(
                    spiBus: Device.CreateSpiBus(),
                    chipSelectPin: Device.Pins.D03,
                    dcPin: Device.Pins.D02,
                    resetPin: Device.Pins.D01,
                    busyPin: Device.Pins.D00),

                _ => null
            };

            return display;
        }

        //<!=SNOP=>
    }
}