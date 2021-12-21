using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace FeatherWings.DotstarWing_Sample
{
    /// <remarks>NOTE: The dotstar feather by default is not connected to the SPI MOSI or SCK pins. 
    /// https://learn.adafruit.com/adafruit-dotstar-featherwing-adafruit/pinouts
    /// </remarks>
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        DotstarWing dotStarWing;
        MicroGraphics graphics;
        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            ISpiBus spiBus = Device.CreateSpiBus();

            dotStarWing = new DotstarWing(spiBus);

            dotStarWing.Brightness = 0.1f;

            graphics = new MicroGraphics(dotStarWing);
            graphics.CurrentFont = new Font4x6();

            graphics.DrawRectangle(0, 0, 8, 4, Color.LawnGreen, true);
            graphics.DrawRectangle(2, 2, 8, 4, Color.Cyan, true);
            graphics.DrawText(0, 0, "F7", Color.White);

            graphics.Show();
        }

        //<!—SNOP—>

        void DrawPixels()
        {
            Console.WriteLine("DrawPixels...");
            dotStarWing.Clear(true);
            Thread.Sleep(1000);

            dotStarWing.DrawPixel(0, 0, Color.Red);
            dotStarWing.DrawPixel(11, 0, Color.White);
            dotStarWing.DrawPixel(0, 5, Color.Blue);
            dotStarWing.DrawPixel(11, 5, Color.Green);

            dotStarWing.Show();
        }

        void GoThroughEachPixel()
        {
            Console.WriteLine("Go Through Each Pixel...");
            dotStarWing.Clear(true);

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.White);
            colors.Add(Color.Blue);
            colors.Add(Color.Green);
            colors.Add(Color.Yellow);
            colors.Add(Color.Purple);
            Random random = new Random();

            for (int y = 0; y< dotStarWing.Height; y++)
            {
                for(int x = 0; x < dotStarWing.Width; x++)
                {
                    int rnd = random.Next(0, colors.Count);
                    dotStarWing.Clear();
                    dotStarWing.DrawPixel(x, y, colors[rnd]);
                    dotStarWing.Show();
                    Thread.Sleep(75);
                }
            }
        }

        void ScrollText()
        {
            Console.WriteLine("ScrollText...");

            dotStarWing.Clear();

            string text = "MEADOW";
            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.White);
            colors.Add(Color.Blue);
            colors.Add(Color.Green);
            colors.Add(Color.Yellow);
            colors.Add(Color.Purple);

            int x = 0;
            int colorIndex = 0;
            int scollWidth = (int)(-1 * (dotStarWing.Width + graphics.CurrentFont.Width + 8));
            int resetWidth = (int)(dotStarWing.Width + 2);

            while (true)
            {
                graphics.Clear();
                int offset = 0;
                colorIndex = 0;

                foreach (var chr in text)
                {
                    graphics.DrawText(x + offset, 0, chr.ToString(), colors[colorIndex]);
                    offset += graphics.CurrentFont.Width;
                    colorIndex++;

                    if (colorIndex >= colors.Count)
                        colorIndex = 0;
                }

                graphics.Show();

                x--;

                if (x < scollWidth)
                    x = resetWidth;

                Thread.Sleep(175);
            }
        }
    }
}