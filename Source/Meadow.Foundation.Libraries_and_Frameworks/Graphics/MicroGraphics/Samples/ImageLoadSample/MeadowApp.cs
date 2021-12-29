using System;
using System.Diagnostics;
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

    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        MicroGraphics _graphics;
        St7789 _display;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            var config = new SpiClockConfiguration(new Frequency(48000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

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

            Meadow.Foundation.Graphics.
//            _graphics.DrawBitmap

            _graphics = new MicroGraphics(display);
            _graphics.Rotation = RotationType._180Degrees;

            _graphics.Clear(true);

        }
    }
}