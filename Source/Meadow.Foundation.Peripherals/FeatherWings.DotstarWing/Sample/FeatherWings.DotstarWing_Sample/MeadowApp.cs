using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace FeatherWings.DotstarWing_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        DotstarWing _dotStarWing;
        GraphicsLibrary _graphics;

        public MeadowApp()
        {
            Initialize();
            DrawPixels();
            GoThroughEachPixel();
            ScrollText();
        }

        /// <remarks>NOTE: The dotstar feather by default is not connected to the SPI MOSI or SCK pins. 
        /// https://learn.adafruit.com/adafruit-dotstar-featherwing-adafruit/pinouts
        /// </remarks>
        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            ISpiBus spiBus = Device.CreateSpiBus();
            IDigitalOutputPort spiPeriphChipSelect = Device.CreateDigitalOutputPort(Device.Pins.D04);
            
            _dotStarWing = new DotstarWing(spiBus, spiPeriphChipSelect);
            
            _graphics = new GraphicsLibrary(_dotStarWing);
            _graphics.CurrentFont = new Font4x8();
            
            _dotStarWing.SetPenColor(Color.Blue);
            _dotStarWing.Brightness = 0.1f;
        }

        void DrawPixels()
        {
            Console.WriteLine("DrawPixels...");
            _dotStarWing.Clear(true);
            Thread.Sleep(1000);

            _dotStarWing.DrawPixel(0, 0, Color.Red);
            _dotStarWing.DrawPixel(11, 0, Color.White);
            _dotStarWing.DrawPixel(0, 5, Color.Blue);
            _dotStarWing.DrawPixel(11, 5, Color.Green);

            _dotStarWing.Show();

        }

        void GoThroughEachPixel()
        {
            Thread.Sleep(2000);
            Console.WriteLine("Go Through Each Pixel...");
            _dotStarWing.Clear(true);

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.White);
            colors.Add(Color.Blue);
            colors.Add(Color.Green);
            colors.Add(Color.Yellow);
            colors.Add(Color.Purple);
            Random random = new Random();

            for (int y = 0; y< _dotStarWing.Height; y++)
            {
                for(int x = 0; x < _dotStarWing.Width; x++)
                {
                    int rnd = random.Next(0, colors.Count);
                    _dotStarWing.Clear();
                    _dotStarWing.DrawPixel(x, y, colors[rnd]);
                    _dotStarWing.Show();
                    Thread.Sleep(75);
                }
            }

        }

        void ScrollText()
        {
            Thread.Sleep(2000);
            Console.WriteLine("ScrollText...");

            _dotStarWing.Clear();

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
            int scollWidth = (int)(-1 * (_dotStarWing.Width + _graphics.CurrentFont.Width + 8));
            int resetWidth = (int)(_dotStarWing.Width + 2);

            while (true)
            {
                _graphics.Clear();
                int offset = 0;
                colorIndex = 0;

                foreach (var chr in text)
                {
                    _graphics.DrawText(x + offset, 0, chr.ToString(), colors[colorIndex]);
                    offset += _graphics.CurrentFont.Width;
                    colorIndex++;

                    if (colorIndex >= colors.Count)
                        colorIndex = 0;
                }

                _graphics.Show();

                x--;

                if (x < scollWidth)
                    x = resetWidth;

                Thread.Sleep(175);
            }

        }
    }
}
