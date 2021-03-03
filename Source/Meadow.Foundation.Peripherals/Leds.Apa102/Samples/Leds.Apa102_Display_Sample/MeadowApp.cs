using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace Leds.Apa102_Display_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Apa102 display;

        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();
        
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            display = new Apa102(Device.CreateSpiBus(), Apa102.PixelOrder.BGR, false, 32, 8);

            graphics = new GraphicsLibrary(display);

            graphics.CurrentFont = new Font4x8();

            graphics.Clear();
            graphics.DrawText(0, 1, "MEADOW", Color.FromRgb(0, 0.1, 0.1));
            graphics.DrawText(24, 1, "F7", Color.FromRgb(0.1, 0, 0));
            graphics.Show();
        }
    }
}