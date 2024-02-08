using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Graphics
{
    /* 
       AISU:
        chipSelectPin: Device.Pins.D15,
        dcPin: Device.Pins.D11,
        resetPin: Device.Pins.D14, 
       JUEGO:
        chipSelectPin: Device.Pins.D14,
        dcPin: Device.Pins.D03,
        resetPin: Device.Pins.D04,
    */

    public class MeadowApp : App<F7FeatherV2>
    {
        private MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var config = new SpiClockConfiguration(new Frequency(48000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            var display = new St7789(
                spiBus: spiBus,
                chipSelectPin: Device.Pins.A03,
                dcPin: Device.Pins.A04,
                resetPin: Device.Pins.A05,
                width: 240, height: 240, colorMode: ColorMode.Format16bppRgb565)
            {
            };

            graphics = new MicroGraphics(display)
            {
                Rotation = RotationType._180Degrees,
                IgnoreOutOfBoundsPixels = true
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            var delay = TimeSpan.FromMilliseconds(1000);

            graphics.Clear();
            graphics.CurrentFont = new Font12x20();

            graphics.DrawText(5, 200, "starting...", Color.White);
            graphics.Show();
            Thread.Sleep(delay);

            int x = 0;
            int y = 0;

            while (true)
            {
                DrawImageFromFile(8, x, y);
                Thread.Sleep(delay);
                DrawImageFromResource(8, x, y);
                Thread.Sleep(delay);
                DrawImageFromFile(24, x, y);
                Thread.Sleep(delay);
                DrawImageFromResource(24, x, y);
                Thread.Sleep(delay);

                x += 1;
                y += 1;

                if (x > 260) x = -20;
                if (y > 260) y = -20;
            }
        }

        private void DrawImageFromFile(int depth, int x = 0, int y = 0)
        {
            Resolver.Log.Info("Showing file...");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, $"wl{depth}.bmp");
            var image = Image.LoadFromFile(filePath);
            graphics.Clear();
            graphics.DrawImage(x, y, image);
            graphics.DrawText(5, 200, $"{depth}bpp file", Color.White);
            graphics.Show();
        }

        private void DrawImageFromResource(int depth, int x = 0, int y = 0)
        {
            Resolver.Log.Info("Showing resource...");
            var image = Image.LoadFromResource($"wl{depth}_res.bmp");
            graphics.Clear();
            graphics.DrawImage(x, y, image);
            graphics.DrawText(5, 200, $"{depth}bpp resource", Color.White);
            graphics.Show();
        }
    }
}