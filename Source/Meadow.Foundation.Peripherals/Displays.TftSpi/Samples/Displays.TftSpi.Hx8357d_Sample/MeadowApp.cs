using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace Displays.Tft.Hx8357d_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        TftSpiBase display;

        public MeadowApp()
        {
            Console.WriteLine("TftSpi sample");

            Initialize();

            Console.WriteLine("Clear");
            display.Clear();
            display.Show();

            Console.WriteLine("Small rect");
            graphics.Clear();
            graphics.DrawRectangle(0, 0, 30, 30, Color.Azure, true);
            graphics.Show();
            Thread.Sleep(1000);

            OverviewScreen();
            Thread.Sleep(5000);

            while (true)
            {
                InvertTest();

                PolarLineTest();
                Thread.Sleep(5000);

                RoundRectTest();
                Thread.Sleep(5000);

                QuadrantTest();
                Thread.Sleep(5000);

                StrokeTest();
                Thread.Sleep(5000);

                ShapeTest();
                Thread.Sleep(5000);

                FontScaleTest();
                Thread.Sleep(5000);

                ColorFontTest();
                Thread.Sleep(5000);

                CircleTest();
                Thread.Sleep(5000);
            }
        }
        void Initialize()
        {
            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode0);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new Hx8357d(device: Device, spiBus: spiBus,
                resetPin: Device.Pins.D00,
                dcPin: Device.Pins.D01,
                chipSelectPin: Device.Pins.D02,
                width: 320, height: 480);

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);

            Console.WriteLine("Init complete");
        }

        void InvertTest()
        {
            graphics.CurrentFont = new Font12x16();
            graphics.Clear();

            string msg = "Cursor test";
            string msg2 = "$123.456";

            graphics.DrawText(0, 1, msg, Color.AliceBlue);
            graphics.DrawRectangle(0, 16, 12 * msg2.Length, 16, Color.DarkSlateBlue, true);
            graphics.DrawText(0, 16, msg2, Color.GreenYellow);

            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 6; j++)
                {
                    graphics.InvertRectangle(i * 12, 0, 12, 16);
          
                    graphics.Show();
                    Thread.Sleep(50);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    graphics.InvertRectangle(i * 12, 16, 12, 16);

                    graphics.Show();
                    Thread.Sleep(50);
                }
            }
        }

        void OverviewScreen()
        {
            Console.WriteLine("Show overview");

            graphics.CurrentFont = new Font12x16();
            graphics.Clear();
            graphics.Stroke = 1;

            graphics.DrawText(0, 0, "HX8357D controller", Color.White, GraphicsLibrary.ScaleFactor.X2);
            graphics.DrawText(0, 30, "320x480 resolution", Color.LawnGreen, GraphicsLibrary.ScaleFactor.X2);
            graphics.DrawText(0, 60, "12 or 16 bit color", Color.AliceBlue, GraphicsLibrary.ScaleFactor.X2);

            for(int i = 0; i < 16; i++)
            {
                graphics.DrawRectangle( 90, i * 20, 20, 30, Color.FromRgb(i * 16, 0, 0));
                graphics.DrawRectangle(120, i * 20, 20, 30, Color.FromRgb(i * 16, i * 16, 0));
                graphics.DrawRectangle(150, i * 20, 20, 30, Color.FromRgb(0,  i * 16, 0));
                graphics.DrawRectangle(180, i * 20, 20, 30, Color.FromRgb(0, i * 16, i * 16));
                graphics.DrawRectangle(210, i * 20, 20, 30, Color.FromRgb(0, 0, i * 16));
                graphics.DrawRectangle(240, i * 20, 20, 30, Color.FromRgb(i * 16, 0, i * 16));
                graphics.DrawRectangle(270, i * 20, 20, 30, Color.FromRgb(i * 16, i * 16, i * 16));
            }

            graphics.Show();

            Console.WriteLine("Show overview complete");
        }

        void PolarLineTest()
        {
            graphics.Clear();
            graphics.Stroke = 1;

            for (int i = 0; i < 270; i+= 12)
            {
                graphics.DrawLine(120, 120, 80, (float)(i * Math.PI / 180), Color.White);
            }

            graphics.Show();
        }

        void RoundRectTest()
        {
            graphics.Clear();

            graphics.Stroke = 1;

            graphics.DrawRoundedRectangle(10, 10, 200, 200, 20, Color.Orange, false);

            graphics.DrawRoundedRectangle(40, 40, 100, 60, 20, Color.Blue, true);

            graphics.DrawRoundedRectangle(100, 70, 60, 60, 20, Color.LawnGreen, true);

            graphics.Show();
        }

        void QuadrantTest()
        {
            graphics.Clear();

            graphics.DrawCircleQuadrant(120, 120, 110, 0, Color.Yellow, true);
            graphics.DrawCircleQuadrant(120, 120, 110, 1, Color.Blue, true);
            graphics.DrawCircleQuadrant(120, 120, 110, 2, Color.Cyan, true);
            graphics.DrawCircleQuadrant(120, 120, 110, 3, Color.LawnGreen, true);

            graphics.Show();
        }

        void CircleTest()
        {
            graphics.Clear();

            graphics.Stroke = 1;
            graphics.DrawCircle(120, 120, 20, Color.Purple);

            graphics.Stroke = 2;
            graphics.DrawCircle(120, 120, 30, Color.Red);

            graphics.Stroke = 3;
            graphics.DrawCircle(120, 120, 40, Color.Orange);

            graphics.Stroke = 4;
            graphics.DrawCircle(120, 120, 50, Color.Yellow);

            graphics.Stroke = 5;
            graphics.DrawCircle(120, 120, 60, Color.LawnGreen);

            graphics.Stroke = 6;
            graphics.DrawCircle(120, 120, 70, Color.Cyan);

            graphics.Stroke = 7;
            graphics.DrawCircle(120, 120, 80, Color.Blue);

            graphics.Show();
        }

        void ShapeTest()
        {
            graphics.Clear();

            graphics.DrawCircle(60, 60, 20, Color.Purple);
            graphics.DrawRectangle(10, 10, 30, 60, Color.Red);
            graphics.DrawTriangle(20, 20, 10, 70, 60, 60, Color.Green);

            graphics.DrawCircle(90, 60, 20, Color.Cyan, true);
            graphics.DrawRectangle(100, 100, 30, 10, Color.Yellow, true);
            graphics.DrawTriangle(120, 20, 110, 70, 160, 60, Color.Pink, true);

            graphics.DrawLine(10, 120, 110, 130, Color.SlateGray);

            graphics.Show();
        }

        void StrokeTest()
        {
            graphics.Clear();

            graphics.Stroke = 1;
            graphics.DrawLine(5, 5,  115, 5,  Color.SteelBlue);
            graphics.Stroke = 2;
            graphics.DrawLine(5, 25, 115, 25, Color.SteelBlue);
            graphics.Stroke = 3;
            graphics.DrawLine(5, 45, 115, 45, Color.SteelBlue);
            graphics.Stroke = 4;
            graphics.DrawLine(5, 65, 115, 65, Color.SteelBlue);
            graphics.Stroke = 5;
            graphics.DrawLine(5, 85, 115, 85, Color.SteelBlue);

            graphics.Stroke = 1;
            graphics.DrawLine(135, 5, 135, 115, Color.SlateGray);
            graphics.Stroke = 2;
            graphics.DrawLine(155, 5, 155, 115, Color.SlateGray);
            graphics.Stroke = 3;
            graphics.DrawLine(175, 5, 175, 115, Color.SlateGray);
            graphics.Stroke = 4;
            graphics.DrawLine(195, 5, 195, 115, Color.SlateGray);
            graphics.Stroke = 5;
            graphics.DrawLine(215, 5, 215, 115, Color.SlateGray);

            graphics.Stroke = 1;
            graphics.DrawLine(5,  125, 115, 235, Color.Silver);
            graphics.Stroke = 2;
            graphics.DrawLine(25, 125, 135, 235, Color.Silver);
            graphics.Stroke = 3;
            graphics.DrawLine(45, 125, 155, 235, Color.Silver);
            graphics.Stroke = 4;
            graphics.DrawLine(65, 125, 175, 235, Color.Silver);
            graphics.Stroke = 5;
            graphics.DrawLine(85, 125, 195, 235, Color.Silver);

            graphics.Stroke = 2;
            graphics.DrawRectangle(2, 2, 236, 236, Color.DimGray, false);

            graphics.Show();
        }

        void FontScaleTest()
        {
            graphics.CurrentFont = new Font12x20();

            graphics.Clear();

            graphics.DrawText(0, 0, "2x Scale", Color.Blue, GraphicsLibrary.ScaleFactor.X2);

            graphics.DrawText(0, 48, "12x20 Font", Color.Green, GraphicsLibrary.ScaleFactor.X2);

            graphics.DrawText(0, 96, "0123456789", Color.Yellow, GraphicsLibrary.ScaleFactor.X2);

            graphics.DrawText(0, 144, "!@#$%^&*()", Color.Orange, GraphicsLibrary.ScaleFactor.X2);

            graphics.DrawText(0, 192, "3x!", Color.OrangeRed, GraphicsLibrary.ScaleFactor.X3);

            graphics.Show();
        }

        void ColorFontTest()
        {
            graphics.CurrentFont = new Font8x12();

            graphics.Clear();

            graphics.DrawTriangle(120, 20, 200, 100, 120, 100, Meadow.Foundation.Color.Red, false);

            graphics.DrawRectangle(140, 30, 40, 90, Meadow.Foundation.Color.Yellow, false);

            graphics.DrawCircle(160, 80, 40, Meadow.Foundation.Color.Cyan, false);

            int indent = 5;
            int spacing = 14;
            int y = indent;

            graphics.DrawText(indent, y, "Meadow F7 SPI ST7789!!");

            graphics.DrawText(indent, y += spacing, "Red", Meadow.Foundation.Color.Red);

            graphics.DrawText(indent, y += spacing, "Purple", Meadow.Foundation.Color.Purple);

            graphics.DrawText(indent, y += spacing, "BlueViolet", Meadow.Foundation.Color.BlueViolet);

            graphics.DrawText(indent, y += spacing, "Blue", Meadow.Foundation.Color.Blue);

            graphics.DrawText(indent, y += spacing, "Cyan", Meadow.Foundation.Color.Cyan);

            graphics.DrawText(indent, y += spacing, "LawnGreen", Meadow.Foundation.Color.LawnGreen);

            graphics.DrawText(indent, y += spacing, "GreenYellow", Meadow.Foundation.Color.GreenYellow);

            graphics.DrawText(indent, y += spacing, "Yellow", Meadow.Foundation.Color.Yellow);

            graphics.DrawText(indent, y += spacing, "Orange", Meadow.Foundation.Color.Orange);

            graphics.DrawText(indent, y += spacing, "Brown", Meadow.Foundation.Color.Brown);

            graphics.Show();

            Console.WriteLine("Show complete");
        }
    }
}