using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading;
using System.Threading.Tasks;

namespace Displays.Ssd130x.Ssd1306_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;
        Ssd1306 display;

        public override Task Initialize()
        {
            //CreateSpiDisplay();
            CreateI2CDisplay();

            graphics = new MicroGraphics(display);

            return base.Initialize();
        }

        void CreateSpiDisplay()
        {
            Resolver.Log.Info("Create Display with SPI...");

            display = new Ssd1306
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                displayType: Ssd1306.DisplayType.OLED128x64
            );
        }

        void CreateI2CDisplay()
        {
            Resolver.Log.Info("Create Display with I2C...");

            display = new Ssd1306
            (
                i2cBus: Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.FastPlus),
                address: 60,
                displayType: Ssd1306.DisplayType.OLED128x32
            );
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7", Meadow.Foundation.Color.White);
            graphics.DrawRectangle(5, 14, 30, 10, true);

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>

        void AdditionalTests()
        {
            Resolver.Log.Info("Fill display");
            for (int x = 0; x < display.Width; x++)
            {
                for (int y = 0; y < display.Height; y++)
                {
                    display.DrawPixel(x, y, true);

                }
            }
            display.Show();
            Thread.Sleep(2000);

            Resolver.Log.Info("Test Inversion");
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 12; y++)
                {
                    display.InvertPixel(x, y);
                }
            }
            display.Show();
            Thread.Sleep(3000);

            Resolver.Log.Info("Check offsets");

            graphics.Clear();
            graphics.DrawRectangle(0, 0, (int)display.Width, (int)display.Height, true, false);
            graphics.Show();

            Thread.Sleep(3000);

            for (int x = 0; x < display.Width; x++)
            {

                Resolver.Log.Info($"X: {x}");
                graphics.Clear();

                graphics.DrawLine(x, 0, x, (int)display.Height - 1, true);

                graphics.Show();

                Thread.Sleep(50);
            }

            for (int y = 0; y < display.Height; y++)
            {
                Resolver.Log.Info($"Y: {y}");
                graphics.Clear();

                graphics.DrawLine(0, y, (int)display.Width - 1, y, true);

                graphics.Show();

                Thread.Sleep(50);
            }

        }

        void TestRawDisplayAPI()
        {
            display.Clear(true);

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            display.Show();
        }
    }
}