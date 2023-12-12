using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize display...");

            var spiBus = Device.CreateSpiBus();

            var display = new Ssd1327(spiBus, Device.Pins.D02, Device.Pins.D01, Device.Pins.D00);

            display.SetContrast(60);

            graphics = new MicroGraphics(display);
            graphics.CurrentFont = new Font8x12();

            return base.Initialize();
        }

        public override Task Run()
        {
            graphics.Clear();

            for (int i = 10; i > 0; i--)
            {   //iterate across different brightnesses
                graphics.DrawText(0, i * 11, "SSD1327", Color.FromRgb(i * 0.1, i * 0.1, i * 0.1));
            }

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>

        void TestDisplay()
        {
            while (true)
            {
                PolarLineTest();
                Thread.Sleep(5000);

                RoundRectTest();
                Thread.Sleep(5000);

                QuadrantTest();
                Thread.Sleep(5000);

                ColorFontTest();
                Thread.Sleep(5000);
            }

        }

        void PolarLineTest()
        {
            graphics.Clear();
            graphics.Stroke = 1;

            for (int i = 0; i < 270; i += 12)
            {
                graphics.DrawLine(64, 64, 60, (float)(i * Math.PI / 180), Color.White);
            }

            graphics.Show();
        }

        void RoundRectTest()
        {
            graphics.Clear();

            graphics.Stroke = 1;

            graphics.DrawRoundedRectangle(10, 10, 100, 100, 10, Color.Orange, false);

            graphics.DrawRoundedRectangle(20, 20, 100, 60, 20, Color.Blue, true);

            graphics.Show();
        }

        void QuadrantTest()
        {
            graphics.Clear();

            graphics.DrawCircleQuadrant(64, 64, 60, 0, Color.Yellow, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 1, Color.Blue, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 2, Color.Cyan, true);
            graphics.DrawCircleQuadrant(64, 64, 60, 3, Color.LawnGreen, true);

            graphics.Show();
        }

        void ColorFontTest()
        {
            graphics.CurrentFont = new Font8x12();

            graphics.Clear();

            graphics.DrawTriangle(120, 20, 200, 100, 120, 100, Color.Red, false);

            graphics.DrawRectangle(140, 30, 40, 90, Color.Yellow, false);

            graphics.DrawCircle(160, 80, 40, Color.Cyan, false);

            int indent = 5;
            int spacing = 14;
            int y = indent;

            graphics.DrawText(indent, y, "Meadow + SSD1327!!");

            graphics.DrawText(indent, y += spacing, "Red", Color.Red);

            graphics.DrawText(indent, y += spacing, "Purple", Color.Purple);

            graphics.DrawText(indent, y += spacing, "BlueViolet", Color.BlueViolet);

            graphics.DrawText(indent, y += spacing, "Blue", Color.Blue);

            graphics.DrawText(indent, y += spacing, "Cyan", Color.Cyan);

            graphics.DrawText(indent, y += spacing, "LawnGreen", Color.LawnGreen);

            graphics.DrawText(indent, y += spacing, "GreenYellow", Color.GreenYellow);

            graphics.DrawText(indent, y += spacing, "Yellow", Color.Yellow);

            graphics.DrawText(indent, y += spacing, "Orange", Color.Orange);

            graphics.DrawText(indent, y += spacing, "Brown", Color.Brown);

            graphics.Show();

            Resolver.Log.Info("Show complete");
        }
    }
}