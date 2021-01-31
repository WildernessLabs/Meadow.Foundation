using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace FeatherWings.CharlieWing_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        CharlieWing charlieWing;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();

            FourCorners();

            Thread.Sleep(2000);
            Face();

            Thread.Sleep(2000);
            ScrollText();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
     
            charlieWing = new CharlieWing(Device.CreateI2cBus());
            charlieWing.Clear();
            charlieWing.Brightness = 128;

            graphics = new GraphicsLibrary(charlieWing);
            graphics.CurrentFont = new Font4x8();
        }

        void FourCorners()
        {
            charlieWing.Frame = 0;
            charlieWing.Clear();
            charlieWing.DrawPixel(0, 0);
            charlieWing.DrawPixel(14, 0);
            charlieWing.DrawPixel(0, 6);
            charlieWing.DrawPixel(14, 6);
            charlieWing.Show(0);
        }

        void ScrollText()
        {
            Console.WriteLine("ScrollText...");
            charlieWing.Clear();

            string text = "MEADOW";

            int x = 0;
            byte frameIndex = 0;
            int scollWidth = (int)(-1 * (charlieWing.Width + graphics.CurrentFont.Width + 4));
            int resetWidth = (int)(charlieWing.Width);
            charlieWing.Frame = 0;
            
            while (true)
            {
                charlieWing.Frame = frameIndex;
                graphics.Clear();
                int offset = 0;
                foreach(var chr in text)
                {
                    graphics.DrawText(x + offset, 0, chr.ToString());
                    offset += graphics.CurrentFont.Width;
                }

                graphics.Show();

                frameIndex = (frameIndex == 0) ? (byte)1 : (byte)0;

                x--;

                if (x < scollWidth)
                {
                    x = resetWidth;
                }
            }
        }

        void Face()
        {
            Console.WriteLine("Face...");
            charlieWing.Clear();

            charlieWing.Frame = 0;
            graphics.DrawCircle(6, 3, 3);

            graphics.DrawPixel(5, 2);
            graphics.DrawPixel(7, 2);

            graphics.DrawLine(5, 4, 7, 4, true);

            charlieWing.Show(0);

            charlieWing.Frame = 1;
            graphics.DrawCircle(6, 3, 3);

            graphics.DrawPixel(5, 2);
            graphics.DrawPixel(7, 2);
            graphics.DrawPixel(5, 4);
            graphics.DrawPixel(6, 5);
            graphics.DrawPixel(7, 4);

            byte frameIndex = 0;
            for(int i = 0; i < 10;i++)
            {
                charlieWing.Show(frameIndex);

                frameIndex = (frameIndex == 0) ? (byte)1 : (byte)0;

                Thread.Sleep(1000);
            }
        }
    }
}