using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        MicroGraphics graphics;
        Max7219 display;

        public override Task Initialize()
        {
            Resolver.Log.Info("Init...");

            display = new Max7219(
                Device.CreateSpiBus(),
                Device.Pins.D00, deviceCount: 4,
                maxMode: Max7219.Max7219Mode.Display);

            graphics = new MicroGraphics(display)
            {
                CurrentFont = new Font4x8(),
                IgnoreOutOfBoundsPixels = true,
            };

            Resolver.Log.Info($"Display W: {display.Width}, H: {display.Height}");

            graphics.Rotation = RotationType._90Degrees;

            Resolver.Log.Info($"Graphics W: {graphics.Width}, H: {graphics.Height}");

            Resolver.Log.Info("Max7219 instantiated");

            graphics.Clear();
            graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height);

            graphics.Show();

            Thread.Sleep(2000);

            return base.Initialize();
        }

        void ScrollText()
        {
            graphics.CurrentFont = new Font6x8();

            string message = "Meadow F7 by Wilderness Labs";

            int delta = graphics.MeasureText(message).Width - graphics.Width;

            for (int i = 0; i < delta; i++)
            {
                graphics.Clear();
                graphics.DrawText(0 - i, 0, message);
                graphics.Show();
                Thread.Sleep(50);
            }
        }

        void DrawPixels()
        {
            Resolver.Log.Info("Clear");
            display.Clear();
            Resolver.Log.Info("Draw");
            for (int i = 0; i < 8; i++)
            {
                display.DrawPixel(i, i, true);
            }
            display.Show();
        }

        void ShowText()
        {
            graphics.CurrentFont = new Font4x8();

            //Graphics Lib
            Resolver.Log.Info("Clear");
            graphics.Clear();
            graphics.DrawText(0, 1, "MEADOWF7");
            Resolver.Log.Info("Show");
            graphics.Show();
        }

        void Counter()
        {
            graphics.CurrentFont = new Font8x8();

            for (int i = 0; i < 1000; i++)
            {
                graphics.Clear();
                graphics.DrawText(0, 0, $"{i}");
                graphics.Show();
            }
        }

        public override Task Run()
        {
            while (true)
            {
                ShowText();
                Thread.Sleep(2000);

                ScrollText();
                Thread.Sleep(2000);

                Counter();
                Thread.Sleep(2000);

                DrawPixels();
                Thread.Sleep(2000);
            }
        }
    }
}