using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;

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

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        MicroGraphics _graphics;
        St7789 _display;

        public MeadowApp()
        {
            Console.WriteLine("Getting SPI bus...");

            var config = new SpiClockConfiguration(new Frequency(48000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Creating a Display...");
            var display = new St7789(
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D15,
                dcPin: Device.Pins.D11,
                resetPin: Device.Pins.D14,
                width: 240, height: 240, displayColorMode: ColorType.Format16bppRgb565)
            {
                IgnoreOutOfBoundsPixels = true
            };

            Console.WriteLine("Creating a MicroGraphics...");
            _graphics = new MicroGraphics(display);
            _graphics.Rotation = RotationType._180Degrees;

            _graphics.Clear(true);
            _graphics.CurrentFont = new Font12x20();

            _graphics.DrawText(5, 200, "starting...", Color.White);
            _graphics.Show();
            Thread.Sleep(2000);

            while (true)
            {
                DrawImageFromFile();
                Thread.Sleep(2000);
                DrawImageFromResource();
                Thread.Sleep(2000);
            }
        }

        private void DrawImageFromFile()
        {
            Console.WriteLine("Showing file...");
            var filePath = Path.Combine(MeadowOS.FileSystem.UserFileSystemRoot, "wl24.bmp");
            var image = Image.LoadFromFile(filePath);
            _graphics.Clear(true);
            _graphics.DrawImage(image);
            _graphics.DrawText(5, 200, "file", Color.White);
            _graphics.Show();
        }

        private void DrawImageFromResource()
        {
            Console.WriteLine("Showing resource...");
            var image = Image.LoadFromResource("wl24_res.bmp");
            _graphics.Clear(true);
            _graphics.DrawImage(image);
            _graphics.DrawText(5, 200, "resource", Color.White);
            _graphics.Show();
        }
    }
}