using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;

namespace Displays.Tft.ILI9163_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ILI9341 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var spiBus = Device.CreateSpiBus();

            display = new ILI9341
            (
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D13,
                dcPin: Device.Pins.D14,
                resetPin: Device.Pins.D15,
                width: 240, height: 320
            );

            graphics = new GraphicsLibrary(display);

            while (true)
            {

                CharacterTest();

                Thread.Sleep(30000);

                DrawMeadowLogo();

                Thread.Sleep(30000);

                FontTest();

                Thread.Sleep(30000);

                TestDisplay();

                Thread.Sleep(30000);

                TestDisplay();
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
                if(i < height / 2)
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

            for(int i = 32; i < 254; i++)
            {
                if (i == 127)
                    i += 33;

                if(count >= 18 || i >= 254)
                {
                    Console.WriteLine(msg);

                    graphics.DrawText(12, yPos, msg, Color.LawnGreen);

                    yPos += 24;

                    count = 0;
                    msg = string.Empty;
                }

                msg += (char)(i);
                Console.WriteLine($"i = {i}");
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
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Meadow.Foundation.Color.Blue, false);

            graphics.DrawText(5, 5, "Meadow F7 SPI", Color.White);
            graphics.Show();
        }
    }
}