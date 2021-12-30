using System;
using System.Diagnostics;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using Meadow.Units;

namespace Displays.Tft.ST7789_Sample
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
        //<!—SNIP—>

        MicroGraphics graphics;
        St7789 display;

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

            graphics = new MicroGraphics(display);
            graphics.Rotation = RotationType._180Degrees;

            graphics.Clear(true);

            graphics.DrawRectangle(120, 0, 120, 220, Color.White, true);
            graphics.DrawRectangle(0, 0, 120, 20, Color.Red, true);
            graphics.DrawRectangle(0, 20, 120, 20, Color.Purple, true);
            graphics.DrawRectangle(0, 40, 120, 20, Color.Blue, true);
            graphics.DrawRectangle(0, 60, 120, 20, Color.Green, true);
            graphics.DrawRectangle(0, 80, 120, 20, Color.Yellow, true);
            graphics.DrawRectangle(0, 100, 120, 20, Color.Orange, true);

            graphics.Show();

            BufferRotationTest();
        }

        //<!—SNOP—>

        int sleepDuration = 500;
        void DisplayTest()
        { 

            Thread.Sleep(sleepDuration);

            while (true)
            {
                BufferRotationTest();
                Thread.Sleep(sleepDuration);

                PartialUpdateTest();
                Thread.Sleep(sleepDuration);

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

        void BufferRotationTest()
        {
            var buffer = new BufferRgb888(50, 50);
            var oldRotation = graphics.Rotation;

            graphics.Clear();
            graphics.Rotation = RotationType.Default;
            buffer.Fill(Color.Red);
            graphics.DrawBuffer(10, 10, buffer);

            graphics.Rotation = RotationType._90Degrees;
            buffer.Fill(Color.Green);
            graphics.DrawBuffer(10, 10, buffer);

            graphics.Rotation = RotationType._180Degrees;
            buffer.Fill(Color.Blue);
            graphics.DrawBuffer(10, 10, buffer);

            graphics.Rotation = RotationType._270Degrees;
            buffer.Fill(Color.Yellow);
            graphics.DrawBuffer(10, 10, buffer);

            graphics.Show();

            graphics.Rotation = oldRotation;
        }

        void PartialUpdateTest()
        {
            var rand = new Random();
            int x, y;

            graphics.Clear(true);

            for(int i = 0; i < 200; i++)
            {
                if(i == 0) graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height, Color.Blue, true);
                if (i == 50) graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height, Color.LawnGreen, true);
                if (i == 100) graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height, Color.Cyan, true);
                if (i == 150) graphics.DrawRectangle(0, 0, graphics.Width, graphics.Height, Color.Yellow, true);

                x = rand.Next() % 23 * 10;
                y = rand.Next() % 23 * 10;

                graphics.Show(x, y, x + 10, y + 10);
            }
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

            graphics.Clear();

            graphics.Stroke = 3;
            graphics.DrawLine(0, 120, 240, 120, Color.White);
            graphics.DrawPath(pathSin, Color.Cyan);
            graphics.DrawPath(pathCos, Color.LawnGreen);

            graphics.Show();
        }

        void FontAlignmentTest()
        {
            graphics.Clear();

            graphics.DrawText(120, 0, "Left aligned", Color.Blue);
            graphics.DrawText(120, 16, "Center aligned", Color.Green, ScaleFactor.X1, TextAlignment.Center);
            graphics.DrawText(120, 32, "Right aligned", Color.Red, ScaleFactor.X1, TextAlignment.Right);

            graphics.DrawText(120, 64, "Left aligned", Color.Blue, ScaleFactor.X2);
            graphics.DrawText(120, 96, "Center aligned", Color.Green, ScaleFactor.X2, TextAlignment.Center);
            graphics.DrawText(120, 128, "Right aligned", Color.Red, ScaleFactor.X2, TextAlignment.Right);

            graphics.Show();
        }

        void InvertTest()
        {
            graphics.CurrentFont = new Font12x16();
            graphics.Clear();

            string msg = "Cursor test";
            string msg2 = "$123.456";

            graphics.DrawText(0, 0, msg, WildernessLabsColors.AzureBlue);
            graphics.DrawRectangle(0, 16, 12 * msg2.Length, 16, WildernessLabsColors.AzureBlueDark, true);
            graphics.DrawText(0, 16, msg2, WildernessLabsColors.ChileanFire);

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

        void LineTest()
        {
            Console.WriteLine("Horizonal lines");

            graphics.Clear();

            for (int i = 1; i < 10; i++)
            {
                graphics.Stroke = i;
                graphics.DrawHorizontalLine(5, 20 * i, (graphics.Width - 10), Color.Red);
            }
            graphics.Show();
            Thread.Sleep(1500);

            graphics.Clear();
            Console.WriteLine("Horizonal lines (negative)");
            for (int i = 1; i < 10; i++)
            {
                graphics.Stroke = i;
                graphics.DrawHorizontalLine((int)graphics.Width - 5, 20 * i, (int)(10 - graphics.Width), Color.Green);
            }
            graphics.Show();
            Thread.Sleep(1500);
            graphics.Clear();

            Console.WriteLine("Vertical lines");

            graphics.Clear();

            for (int i = 1; i < 10; i++)
            {
                graphics.Stroke = i;
                graphics.DrawVerticalLine(20 * i, 5, (int)(graphics.Height - 10), Color.Orange);
            }
            graphics.Show();
            Thread.Sleep(1500);
            graphics.Clear();

            Console.WriteLine("Vertical lines (negative)");
            for (int i = 1; i < 10; i++)
            {
                graphics.Stroke = i;
                graphics.DrawVerticalLine(20 * i, (int)(graphics.Height - 5), (int)(10 - graphics.Width), Color.Blue);
            }
            graphics.Show();
            Thread.Sleep(1500);
        }

        void PolarLineTest()
        {
            graphics.Clear();
            graphics.Stroke = 3;

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

            graphics.DrawRoundedRectangle(10, 10, 200, 200, 20, WildernessLabsColors.ChileanFire, false);

            graphics.DrawRoundedRectangle(40, 40, 100, 60, 20, WildernessLabsColors.AzureBlue, true);

            graphics.DrawRoundedRectangle(100, 70, 60, 60, 20, WildernessLabsColors.PearGreen, true);

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
            graphics.DrawRectangle(2, 2, (int)(graphics.Width - 4), (int)(graphics.Height - 4), Color.DimGray, false);

            graphics.Show();
        }

        void FontScaleTest()
        {
            graphics.CurrentFont = new Font12x20();

            graphics.Clear();

            graphics.DrawText(0, 0, "2x Scale", Color.Blue, ScaleFactor.X2);

            graphics.DrawText(0, 48, "12x20 Font", Color.Green, ScaleFactor.X2);

            graphics.DrawText(0, 96, "0123456789", Color.Yellow, ScaleFactor.X2);

            graphics.DrawText(0, 144, "!@#$%^&*()", Color.Orange, ScaleFactor.X2);

            graphics.DrawText(0, 192, "3x!", Color.OrangeRed, ScaleFactor.X3);

            graphics.DrawText(0, 240, "Meadow!", Color.Red, ScaleFactor.X2);

            graphics.DrawText(0, 288, "B4.2", Color.Violet, ScaleFactor.X2);

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