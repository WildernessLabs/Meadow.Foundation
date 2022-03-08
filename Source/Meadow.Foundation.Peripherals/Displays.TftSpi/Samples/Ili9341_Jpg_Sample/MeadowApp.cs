using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using SimpleJpegDecoder;

namespace Displays.TftSpi.Ili9341_Jpg_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        Ili9341 display;
        MicroGraphics graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var spiBus = Device.CreateSpiBus(Ili9341.DefaultSpiBusSpeed);

            display = new Ili9341
            (
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D13,
                dcPin: Device.Pins.D14,
                resetPin: Device.Pins.D15,
                width: 240, height: 320
            )
            {
                IgnoreOutOfBoundsPixels = true
            };

            graphics = new MicroGraphics(display);

            int delay = 5000;

            while (true)
            {
                JpegTest();

                Thread.Sleep(delay);

                CharacterTest();

                Thread.Sleep(delay);

                DrawMeadowLogo();

                Thread.Sleep(delay);

                FontTest();

                Thread.Sleep(delay);

                TestDisplay();

                Thread.Sleep(delay);

                TestDisplay();
            }
        }

        void JpegTest()
        {
            Console.WriteLine("Jpeg Test");

            var jpgData = LoadResource("meadow.jpg");

            Console.WriteLine($"Loaded {jpgData.Length} bytes, decoding jpeg ...");

            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes");
            Console.WriteLine($"Width {decoder.Width}");
            Console.WriteLine($"Height {decoder.Height}");

            var jpgImage = new BufferRgb888(decoder.Width, decoder.Height, jpg);

            graphics.Clear();
            graphics.DrawRectangle(0, 0, 240, 320, Color.White, true);

            int x = 0;
            int y = (240 - decoder.Height) / 2;

            display.DrawBuffer(x, y, jpgImage);

            Console.WriteLine("Jpeg show");

            display.Show();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Displays.TftSpi.Ili9341_Jpg_Sample.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        void DrawMeadowLogo()
        {
            graphics.Clear();

            var bottom = 200;
            var height = 54;

            graphics.DrawLine(4, bottom, 44, bottom - height, Color.White);
            graphics.DrawLine(4, bottom, 44, bottom, Color.White);
            graphics.DrawLine(44, 200 - height, 64, bottom - height / 2, Color.White);
            graphics.DrawLine(44, bottom, 84, bottom - height, Color.White);
            graphics.DrawLine(84, bottom - height, 124, bottom, Color.White);

            //mountain fill
            int lineWidth, x, y;

            for (int i = 0; i < height - 1; i++)
            {
                y = bottom - i;
                x = 5 + i * 20 / 27;

                //fill bottom of mountain
                if (i < height / 2)
                {
                    lineWidth = 38;
                    graphics.DrawLine(x, y, x + lineWidth, y, Color.YellowGreen);
                }
                else
                { //fill top of mountain
                    lineWidth = 38 - (i - height / 2) * 40 / 27;
                    graphics.DrawLine(x, y, x + lineWidth, y, Color.YellowGreen);
                }
            }

            graphics.Show();

        }

        void CharacterTest()
        {
            graphics.Clear();

            graphics.CurrentFont = new Font12x20();

            string msg = string.Empty;

            int yPos = 12;
            int count = 0;

            for (int i = 32; i < 254; i++)
            {
                if (i == 127)
                    i += 33;

                if (count >= 18 || i >= 254)
                {
                    graphics.DrawText(12, yPos, msg, Color.LawnGreen);

                    yPos += 24;

                    count = 0;
                    msg = string.Empty;
                }

                msg += (char)(i);
                count++;
            }

            graphics.Show();
        }

        void FontTest()
        {
            graphics.Clear();

            int yPos = 0;

            graphics.CurrentFont = new Font4x8();
            graphics.DrawText(0, yPos, "Font_4x8: ABCdef123@#$", Color.Red);
            yPos += 12;

            graphics.CurrentFont = new Font8x8();
            graphics.DrawText(0, yPos, "Font_8x8: ABCdef123@#$", Color.Orange);
            yPos += 12;

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, yPos, "Font_8x12: ABCdef123@#$", Color.Yellow);
            yPos += 16;

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, yPos, "Font_12x16: ABCdef123@#$", Color.LawnGreen);
            yPos += 20;

            graphics.CurrentFont = new Font12x20();
            graphics.DrawText(0, yPos, "Font_12x20: ABCdef123@#$", Color.Cyan);
            yPos += 22;

            graphics.Show();
        }

        void TestDisplay()
        {
            //force a collection
            GC.Collect();

            Console.WriteLine("Draw");

            for (int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, 120 + i, true);
                display.DrawPixel(30 + i, 120 + i, true);
                display.DrawPixel(60 + i, 120 + i, true);
            }

            // Draw with Display Graphics Library
            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Color.Blue, false);

            graphics.DrawText(5, 5, "Meadow F7 SPI", Color.White);
            graphics.Show();
        }
    }
}