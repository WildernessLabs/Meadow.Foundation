using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using static Meadow.Foundation.Displays.DisplayBase;

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
        bool rendering = false;

        public MeadowApp()
        {
            Initialize();

            if (hasDisplay) { Render(new JoystickPosition(0f, 0f)); }

            // assume that the stick is in the center when it starts up
            _ = joystick.SetCenterPosition(); //fire and forget


            //==== Classic Events
            joystick.Updated += JoystickUpdated;

            //==== IObservable
            joystick.StartUpdating(TimeSpan.FromMilliseconds(20));
        }

        void Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            //==== Joystick
            // these are pretty fast updates (40ms in total), if you need more time to process, you can
            // increase the sample interval duration and/or standby duration.
            joystick = new AnalogJoystick(
                Device.CreateAnalogInputPort(Device.Pins.A01, 1, 10),
                Device.CreateAnalogInputPort(Device.Pins.A00, 1, 10),
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

        void JoystickUpdated(object sender, IChangeResult<JoystickPosition> e)
        {
            Console.WriteLine($"Horizontal: {e.New.Horizontal:n2}, Vertical: {e.New.Vertical:n2}");
            if (hasDisplay) { Render(e.New); }
        }

        void Render(JoystickPosition position)
        {
            if(rendering) { Console.WriteLine("already rendering"); return; }

            rendering = true;

            // this seems to be slower, but need to profile
            //Device.BeginInvokeOnMainThread(() => {
            //    int crossWidth = 11;
            //    int crossHeight = crossWidth;
            //    Color blue = Color.FromHex("#23abe3");
            //    Color green = Color.FromHex("#C9DB31");
            //    Color orange = Color.FromHex("#EF7D3B");

            //    // calculate the centerpoint
            //    (int X, int Y) centerPoint = (0, 0);
            //    // NOTE: Juego has its joystick rotated 90° counter clockwise,
            //    // so Horizontal is Vertical
            //    centerPoint.X = (int)position.Vertical.Map(-1, 1, 0, displayWidth);
            //    centerPoint.Y = (int)(-position.Horizontal).Map(-1, 1, 0, displayHeight);

            //    // clear the buffer
            //    canvas.Clear();

            //    // draw the cross lines
            //    canvas.DrawHorizontalLine(centerPoint.X - (crossWidth / 2), centerPoint.Y, crossWidth, blue);
            //    canvas.DrawVerticalLine(centerPoint.X, centerPoint.Y - (crossHeight / 2), crossHeight, blue);
            //    canvas.DrawCircle(centerPoint.X, centerPoint.Y, crossWidth / 2 + 2, orange);

            //    // draw our coordinates
            //    canvas.CurrentFont = new Font12x20();
            //    canvas.DrawText(0, 0, $"X: {centerPoint.X:N0} Y:{centerPoint.Y:N0}", green);

            //    // blit the buffer to the screen
            //    canvas.Show();

            //    rendering = false;

            //});

            Task t = new Task(() => {
                int crossWidth = 11;
                int crossHeight = crossWidth;
                Color blue = Color.FromHex("#23abe3");
                Color green = Color.FromHex("#C9DB31");
                Color orange = Color.FromHex("#EF7D3B");

                // calculate the centerpoint
                (int X, int Y) centerPoint = (0, 0);
                // NOTE: Juego has its joystick rotated 90° counter clockwise,
                // so Horizontal is Vertical
                if (position.Vertical is { } vertical) {
                    centerPoint.X = (int)vertical.Map(-1, 1, 0, displayWidth);
                } else { centerPoint.X = 0.Map(-1, 1, 0, displayWidth); }
                if (position.Horizontal is { } horizontal) {
                    centerPoint.Y = (int)(-horizontal).Map(-1, 1, 0, displayHeight);
                } else { centerPoint.Y = 0.Map(-1, 1, 0, displayHeight); }

                // clear the buffer
                canvas.Clear();

                // draw the cross lines
                canvas.DrawHorizontalLine(centerPoint.X - (crossWidth / 2), centerPoint.Y, crossWidth, blue);
                canvas.DrawVerticalLine(centerPoint.X, centerPoint.Y - (crossHeight / 2), crossHeight, blue);
                canvas.DrawCircle(centerPoint.X, centerPoint.Y, crossWidth / 2 + 2, orange);

                // draw our coordinates
                canvas.CurrentFont = new Font12x20();
                canvas.DrawText(0, 0, $"X: {centerPoint.X:N0} Y:{centerPoint.Y:N0}", green);

                // blit the buffer to the screen
                canvas.Show();

                rendering = false;
            });
            t.Start();
        }
    }
}