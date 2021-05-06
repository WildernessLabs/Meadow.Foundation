using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Hardware;
using static Meadow.Foundation.Displays.DisplayBase;
using Meadow.Peripherals.Sensors.Hid;
using Meadow.Foundation;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //==== peripherals
        AnalogJoystick joystick;
        GraphicsLibrary canvas;
        TftSpiBase display;

        //==== internals
        bool hasDisplay = true; // Set to false if you don't have a display hooked up
        int displayWidth = 240;
        int displayHeight = 240;

        public MeadowApp()
        {
            Initialize();

            if (hasDisplay) { DrawCrosshairs(new JoystickPosition(0f, 0f)); }

            _ = joystick.SetCenterPosition(); //fire and forget 
            joystick.Updated += JoystickUpdated;
            joystick.StartUpdating();
        }

        void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            //==== Joystick
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01),
                Device.CreateAnalogInputPort(Device.Pins.A00),
                null, false);

            //==== Display and graphics
            if (hasDisplay) {
                // SPI Bus
                Console.WriteLine("Create Spi bus");
                var config = new SpiClockConfiguration(48000, SpiClockConfiguration.Mode.Mode3);
                var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

                // Display. Make sure to change the pins here to match your display wireup
                Console.WriteLine("Create display driver instance");
                display = new St7789(device: Device, spiBus: spiBus,
                    chipSelectPin: Device.Pins.D14,
                    dcPin: Device.Pins.D03,
                    resetPin: Device.Pins.D04,
                    width: displayWidth, height: displayHeight, displayColorMode: DisplayColorMode.Format12bppRgb444);

                // graphics library
                Console.WriteLine("Create graphics lib");
                canvas = new GraphicsLibrary(display);
                canvas.Rotation = GraphicsLibrary.RotationType._90Degrees;
            }

            Console.WriteLine("Hardware initialization complete.");
        }

        void JoystickUpdated(object sender, ChangeResult<JoystickPosition> e)
        {
            Console.WriteLine($"Horizontal: {e.New.Horizontal:n2}, Vertical: {e.New.Vertical:n2})");
            if (hasDisplay) { DrawCrosshairs(e.New); }
        }

        void DrawCrosshairs(JoystickPosition position)
        {
            int crossWidth = 11;
            int crossHeight = crossWidth;
            Color blue = Color.FromHex("#23abe3");
            Color green = Color.FromHex("#C9DB31");
            Color orange = Color.FromHex("#EF7D3B");

            // calculate the centerpoint
            (int X, int Y) centerPoint = (0, 0);
            // NOTE: Juego has its joystick rotated 90° counter clockwise,
            // so Horizontal is Vertical
            centerPoint.X = (int)Map(position.Vertical, -1, 1, 0, displayWidth);
            centerPoint.Y = (int)Map(-(position.Horizontal), -1, 1, 0, displayHeight);

            Console.WriteLine($"Centerpoint X:{centerPoint.X}, Y:{centerPoint.Y}");

            canvas.Clear();

            // draw the cross lines
            canvas.DrawHorizontalLine(centerPoint.X - (crossWidth / 2), centerPoint.Y, crossWidth, blue);
            canvas.DrawVerticalLine(centerPoint.X, centerPoint.Y - (crossHeight / 2), crossHeight, blue);
            canvas.DrawCircle(centerPoint.X, centerPoint.Y, crossWidth / 2 + 2, orange);

            // draw our coordinates
            canvas.CurrentFont = new Font12x20();
            canvas.DrawText(0, 0, $"X: {centerPoint.X:N0} Y:{centerPoint.Y:N0}", green);
            canvas.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sourceMin"></param>
        /// <param name="sourceMax"></param>
        /// <param name="targetMin"></param>
        /// <param name="targetMax"></param>
        /// <returns></returns>
        float Map(float value, float sourceMin, float sourceMax, float targetMin, float targetMax)
        {
            return (value - sourceMin) / (sourceMax - sourceMin) * (targetMax - targetMin) + targetMin;
        }
    }
}