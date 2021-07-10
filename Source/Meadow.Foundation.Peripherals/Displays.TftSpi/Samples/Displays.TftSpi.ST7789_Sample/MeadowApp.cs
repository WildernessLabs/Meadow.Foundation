using System;
using System.Diagnostics;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using static Meadow.Foundation.Displays.DisplayBase;

namespace Displays.Tft.ST7789_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary canvas;
        TftSpiBase display;
        int sleepDuration = 500;

        public MeadowApp()
        {
            Console.WriteLine("TftSpi sample");

            Initialize();

            //Benchmark();

            display.ClearScreen(0xFF);
            display.Show();
            Thread.Sleep(sleepDuration);

            canvas.Clear(true);
            
            canvas.DrawRectangle(120, 0, 120, 220, Color.White, true);
            canvas.DrawRectangle(0, 0, 120, 20, Color.Red, true);
            canvas.DrawRectangle(0, 20, 120, 20, Color.Purple, true);
            canvas.DrawRectangle(0, 40, 120, 20, Color.Blue, true);
            canvas.DrawRectangle(0, 60, 120, 20, Color.Green, true);
            canvas.DrawRectangle(0, 80, 120, 20, Color.Yellow, true);
            canvas.DrawRectangle(0, 120, 120, 20, Color.Orange, true); 

            Console.WriteLine("Show");

            canvas.Show();

            Thread.Sleep(sleepDuration);

            while (true)
            {
                PathTest();
                Thread.Sleep(sleepDuration);

                LineTest();
                Thread.Sleep(sleepDuration);

                PolarLineTest();
                Thread.Sleep(sleepDuration);

                RoundRectTest();
                Thread.Sleep(sleepDuration);

                QuadrantTest();
                Thread.Sleep(sleepDuration);

                StrokeTest();
                Thread.Sleep(sleepDuration);

                ShapeTest();
                Thread.Sleep(sleepDuration);

                FontScaleTest();
                Thread.Sleep(sleepDuration);

                FontAlignmentTest();
                Thread.Sleep(sleepDuration);                   

                ColorFontTest();
                Thread.Sleep(sleepDuration);

                CircleTest();
                Thread.Sleep(sleepDuration);

                InvertTest();
                Thread.Sleep(sleepDuration);
            }
        }

        void Benchmark()
        {
            //display.SetPenColor(Color.BlueViolet);

            //for (int x = 0; x < 240; x++)
            //{
            //    for (int y = 0; y < 240; y++)
            //    {
            //        display.DrawPixel(x, y);
            //    }
            //}

            display.Clear(WildernessLabsColors.AzureBlue);

            var sw = new Stopwatch();
            sw.Start();

            for(int i = 0; i < 10; i++)
            {
                display.Show();
            }

            sw.Stop();

            Console.WriteLine($"Elapsed: {sw.Elapsed}s");
            Console.WriteLine($"fps: {10.0/sw.Elapsed.TotalSeconds}");

            Thread.Sleep(1000);
        }

        void Initialize()
        {
            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(48000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new St7789(device: Device, spiBus: spiBus,
                // AISU:
                //chipSelectPin: Device.Pins.D15,//D14,
                //dcPin: Device.Pins.D11,//D03,
                //resetPin: Device.Pins.D14, //D04,
                // JUEGO:
                chipSelectPin: Device.Pins.D14,
                dcPin: Device.Pins.D03,
                resetPin: Device.Pins.D04,
                width: 240, height: 240, displayColorMode: DisplayColorMode.Format12bppRgb444);

            Console.WriteLine("Create graphics lib");

            canvas = new GraphicsLibrary(display);
            canvas.Rotation = GraphicsLibrary.RotationType._180Degrees;

            Console.WriteLine("Init complete");
        }

        void PathTest()
        {
            var pathSin = new GraphicsPath();
            var pathCos = new GraphicsPath();

            for (int i = 0; i < 48; i++)
            {
                if(i == 0)
                {
                    pathSin.MoveTo(0, 120 + (int)(Math.Sin(i * 10 * Math.PI / 180) * 100));
                    pathCos.MoveTo(0, 120 + (int)(Math.Cos(i * 10 * Math.PI / 180) * 100));
                    continue;
                }

                pathSin.LineTo(i * 5, 120 + (int)(Math.Sin(i * 10 * Math.PI / 180) * 100));
                pathCos.LineTo(i * 5, 120 + (int)(Math.Cos(i * 10 * Math.PI / 180) * 100));
            }

            canvas.Clear();

            canvas.Stroke = 3;
            canvas.DrawLine(0, 120, 240, 120, Color.White);
            canvas.DrawPath(pathSin, Color.Cyan);
            canvas.DrawPath(pathCos, Color.LawnGreen);

            canvas.Show();
        }

        void FontAlignmentTest()
        {
            canvas.Clear();

            canvas.DrawText(120, 0, "Left aligned", Color.Blue);
            canvas.DrawText(120, 16, "Center aligned", Color.Green, GraphicsLibrary.ScaleFactor.X1, GraphicsLibrary.TextAlignment.Center);
            canvas.DrawText(120, 32, "Right aligned", Color.Red, GraphicsLibrary.ScaleFactor.X1, GraphicsLibrary.TextAlignment.Right);

            canvas.DrawText(120, 64, "Left aligned", Color.Blue, GraphicsLibrary.ScaleFactor.X2);
            canvas.DrawText(120, 96, "Center aligned", Color.Green, GraphicsLibrary.ScaleFactor.X2, GraphicsLibrary.TextAlignment.Center);
            canvas.DrawText(120, 128, "Right aligned", Color.Red, GraphicsLibrary.ScaleFactor.X2, GraphicsLibrary.TextAlignment.Right);

            canvas.Show();
        }

        void InvertTest()
        {
            canvas.CurrentFont = new Font12x16();
            canvas.Clear();

            string msg = "Cursor test";
            string msg2 = "$123.456";

            canvas.DrawText(0, 1, msg, WildernessLabsColors.AzureBlue);
            canvas.DrawRectangle(0, 16, 12 * msg2.Length, 16, WildernessLabsColors.AzureBlueDark, true);
            canvas.DrawText(0, 16, msg2, WildernessLabsColors.ChileanFire);

            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 6; j++)
                {
                    canvas.InvertRectangle(i * 12, 0, 12, 16);
          
                    canvas.Show();
                    Thread.Sleep(50);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    canvas.InvertRectangle(i * 12, 16, 12, 16);

                    canvas.Show();
                    Thread.Sleep(50);
                }
            }
        }

        void LineTest()
        {
            Console.WriteLine("Horizonal lines");

            canvas.Clear();

            for (int i = 1; i < 10; i++)
            {
                canvas.Stroke = i;
                canvas.DrawHorizontalLine(5, 20 * i, (int)(display.Width - 10), Color.Red);
            }
            canvas.Show();
            Thread.Sleep(1500);

            canvas.Clear();
            Console.WriteLine("Horizonal lines (negative)");
            for (int i = 1; i < 10; i++)
            {
                canvas.Stroke = i;
                canvas.DrawHorizontalLine((int)canvas.Width - 5, 20 * i, (int)(10 - canvas.Width), Color.Green);
            }
            canvas.Show();
            Thread.Sleep(1500);
            canvas.Clear();

            Console.WriteLine("Vertical lines");

            canvas.Clear();

            for (int i = 1; i < 10; i++)
            {
                canvas.Stroke = i;
                canvas.DrawVerticalLine(20 * i, 5, (int)(canvas.Height - 10), Color.Orange);
            }
            canvas.Show();
            Thread.Sleep(1500);
            canvas.Clear();

            Console.WriteLine("Vertical lines (negative)");
            for (int i = 1; i < 10; i++)
            {
                canvas.Stroke = i;
                canvas.DrawVerticalLine(20 * i, (int)(canvas.Height - 5), (int)(10 - canvas.Width), Color.Blue);
            }
            canvas.Show();
            Thread.Sleep(1500);
        }

        void PolarLineTest()
        {
            canvas.Clear();
            canvas.Stroke = 3;

            for (int i = 0; i < 270; i+= 12)
            {
                canvas.DrawLine(120, 120, 80, (float)(i * Math.PI / 180), Color.White);
            }

            canvas.Show();
        }

        void RoundRectTest()
        {
            canvas.Clear();

            canvas.Stroke = 1;

            canvas.DrawRoundedRectangle(10, 10, 200, 200, 20, WildernessLabsColors.ChileanFire, false);

            canvas.DrawRoundedRectangle(40, 40, 100, 60, 20, WildernessLabsColors.AzureBlue, true);

            canvas.DrawRoundedRectangle(100, 70, 60, 60, 20, WildernessLabsColors.PearGreen, true);

            canvas.Show();
        }

        void QuadrantTest()
        {
            canvas.Clear();

            canvas.DrawCircleQuadrant(120, 120, 110, 0, Color.Yellow, true);
            canvas.DrawCircleQuadrant(120, 120, 110, 1, Color.Blue, true);
            canvas.DrawCircleQuadrant(120, 120, 110, 2, Color.Cyan, true);
            canvas.DrawCircleQuadrant(120, 120, 110, 3, Color.LawnGreen, true);

            canvas.Show();
        }

        void CircleTest()
        {
            canvas.Clear();

            canvas.Stroke = 1;
            canvas.DrawCircle(120, 120, 20, Color.Purple);

            canvas.Stroke = 2;
            canvas.DrawCircle(120, 120, 30, Color.Red);

            canvas.Stroke = 3;
            canvas.DrawCircle(120, 120, 40, Color.Orange);

            canvas.Stroke = 4;
            canvas.DrawCircle(120, 120, 50, Color.Yellow);

            canvas.Stroke = 5;
            canvas.DrawCircle(120, 120, 60, Color.LawnGreen);

            canvas.Stroke = 6;
            canvas.DrawCircle(120, 120, 70, Color.Cyan);

            canvas.Stroke = 7;
            canvas.DrawCircle(120, 120, 80, Color.Blue);

            canvas.Show();
        }

        void ShapeTest()
        {
            canvas.Clear();

            canvas.DrawCircle(60, 60, 20, Color.Purple);
            canvas.DrawRectangle(10, 10, 30, 60, Color.Red);
            canvas.DrawTriangle(20, 20, 10, 70, 60, 60, Color.Green);

            canvas.DrawCircle(90, 60, 20, Color.Cyan, true);
            canvas.DrawRectangle(100, 100, 30, 10, Color.Yellow, true);
            canvas.DrawTriangle(120, 20, 110, 70, 160, 60, Color.Pink, true);

            canvas.DrawLine(10, 120, 110, 130, Color.SlateGray);

            canvas.Show();
        }

        void StrokeTest()
        {
            canvas.Clear();

            canvas.Stroke = 1;
            canvas.DrawLine(5, 5,  115, 5,  Color.SteelBlue);
            canvas.Stroke = 2;
            canvas.DrawLine(5, 25, 115, 25, Color.SteelBlue);
            canvas.Stroke = 3;
            canvas.DrawLine(5, 45, 115, 45, Color.SteelBlue);
            canvas.Stroke = 4;
            canvas.DrawLine(5, 65, 115, 65, Color.SteelBlue);
            canvas.Stroke = 5;
            canvas.DrawLine(5, 85, 115, 85, Color.SteelBlue);

            canvas.Stroke = 1;
            canvas.DrawLine(135, 5, 135, 115, Color.SlateGray);
            canvas.Stroke = 2;
            canvas.DrawLine(155, 5, 155, 115, Color.SlateGray);
            canvas.Stroke = 3;
            canvas.DrawLine(175, 5, 175, 115, Color.SlateGray);
            canvas.Stroke = 4;
            canvas.DrawLine(195, 5, 195, 115, Color.SlateGray);
            canvas.Stroke = 5;
            canvas.DrawLine(215, 5, 215, 115, Color.SlateGray);

            canvas.Stroke = 1;
            canvas.DrawLine(5,  125, 115, 235, Color.Silver);
            canvas.Stroke = 2;
            canvas.DrawLine(25, 125, 135, 235, Color.Silver);
            canvas.Stroke = 3;
            canvas.DrawLine(45, 125, 155, 235, Color.Silver);
            canvas.Stroke = 4;
            canvas.DrawLine(65, 125, 175, 235, Color.Silver);
            canvas.Stroke = 5;
            canvas.DrawLine(85, 125, 195, 235, Color.Silver);

            canvas.Stroke = 2;
            canvas.DrawRectangle(2, 2, (int)(canvas.Width - 4), (int)(canvas.Height - 4), Color.DimGray, false);

            canvas.Show();
        }

        void FontScaleTest()
        {
            canvas.CurrentFont = new Font12x20();

            canvas.Clear();

            canvas.DrawText(0, 0, "2x Scale", Color.Blue, GraphicsLibrary.ScaleFactor.X2);

            canvas.DrawText(0, 48, "12x20 Font", Color.Green, GraphicsLibrary.ScaleFactor.X2);

            canvas.DrawText(0, 96, "0123456789", Color.Yellow, GraphicsLibrary.ScaleFactor.X2);

            canvas.DrawText(0, 144, "!@#$%^&*()", Color.Orange, GraphicsLibrary.ScaleFactor.X2);

            canvas.DrawText(0, 192, "3x!", Color.OrangeRed, GraphicsLibrary.ScaleFactor.X3);

            canvas.DrawText(0, 240, "Meadow!", Color.Red, GraphicsLibrary.ScaleFactor.X2);

            canvas.DrawText(0, 288, "B4.2", Color.Violet, GraphicsLibrary.ScaleFactor.X2);

            canvas.Show();
        }

        void ColorFontTest()
        {
            canvas.CurrentFont = new Font8x12();

            canvas.Clear();

            canvas.DrawTriangle(120, 20, 200, 100, 120, 100, Meadow.Foundation.Color.Red, false);

            canvas.DrawRectangle(140, 30, 40, 90, Meadow.Foundation.Color.Yellow, false);

            canvas.DrawCircle(160, 80, 40, Meadow.Foundation.Color.Cyan, false);

            int indent = 5;
            int spacing = 14;
            int y = indent;

            canvas.DrawText(indent, y, "Meadow F7 SPI ST7789!!");

            canvas.DrawText(indent, y += spacing, "Red", Meadow.Foundation.Color.Red);

            canvas.DrawText(indent, y += spacing, "Purple", Meadow.Foundation.Color.Purple);

            canvas.DrawText(indent, y += spacing, "BlueViolet", Meadow.Foundation.Color.BlueViolet);

            canvas.DrawText(indent, y += spacing, "Blue", Meadow.Foundation.Color.Blue);

            canvas.DrawText(indent, y += spacing, "Cyan", Meadow.Foundation.Color.Cyan);

            canvas.DrawText(indent, y += spacing, "LawnGreen", Meadow.Foundation.Color.LawnGreen);

            canvas.DrawText(indent, y += spacing, "GreenYellow", Meadow.Foundation.Color.GreenYellow);

            canvas.DrawText(indent, y += spacing, "Yellow", Meadow.Foundation.Color.Yellow);

            canvas.DrawText(indent, y += spacing, "Orange", Meadow.Foundation.Color.Orange);

            canvas.DrawText(indent, y += spacing, "Brown", Meadow.Foundation.Color.Brown);

            canvas.Show();

            Console.WriteLine("Show complete");
        }
    }
}