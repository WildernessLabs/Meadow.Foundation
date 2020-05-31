using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace FeatherWings.CharlieWing_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        II2cBus _i2cBus;
        CharlieWing _charlieWing;
        GraphicsLibrary _graphics;

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
            _i2cBus = Device.CreateI2cBus();
            _charlieWing = new CharlieWing(_i2cBus);
            _charlieWing.Clear();
            _charlieWing.Brightness = 128;

            _graphics = new GraphicsLibrary(_charlieWing);
            _graphics.CurrentFont = new Font4x8();

        }

        void FourCorners()
        {
            _charlieWing.Frame = 0;
            _charlieWing.Clear();
            _charlieWing.DrawPixel(0, 0);
            _charlieWing.DrawPixel(14, 0);
            _charlieWing.DrawPixel(0, 6);
            _charlieWing.DrawPixel(14, 6);
            _charlieWing.Show(0);
        }

        void ScrollText()
        {
            Console.WriteLine("ScrollText...");
            _charlieWing.Clear();

            string text = "MEADOW";

            int x = 0;
            byte frameIndex = 0;
            int scollWidth = (int)(-1 * (_charlieWing.Width + _graphics.CurrentFont.Width + 4));
            int resetWidth = (int)(_charlieWing.Width);
            _charlieWing.Frame = 0;
            

            while (true)
            {
                _charlieWing.Frame = frameIndex;
                _graphics.Clear();
                int offset = 0;
                foreach(var chr in text)
                {
                    _graphics.DrawText(x + offset, 0, chr.ToString());
                    offset += _graphics.CurrentFont.Width;
                }

                _graphics.Show();
                
                if (frameIndex == 0)
                    frameIndex = 1;
                else
                    frameIndex = 0;

                x--;

                if (x < scollWidth)
                    x = resetWidth;

            }

        }

        void Face()
        {
            Console.WriteLine("Face...");
            _charlieWing.Clear();

            _charlieWing.Frame = 0;
            _graphics.DrawCircle(6, 3, 3);

            _graphics.DrawPixel(5, 2);
            _graphics.DrawPixel(7, 2);

            _graphics.DrawLine(5, 4, 7, 4, true);

            _charlieWing.Show(0);

            _charlieWing.Frame = 1;
            _graphics.DrawCircle(6, 3, 3);

            _graphics.DrawPixel(5, 2);
            _graphics.DrawPixel(7, 2);
            _graphics.DrawPixel(5, 4);
            _graphics.DrawPixel(6, 5);
            _graphics.DrawPixel(7, 4);


            byte frameIndex = 0;
            for(int i = 0; i < 10;i++)
            {
                _charlieWing.Show(frameIndex);

                if (frameIndex == 1)
                    frameIndex = 0;
                else
                    frameIndex = 1;

                Thread.Sleep(1000);
                
            }


        }
    }
}
