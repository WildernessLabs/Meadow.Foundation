using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;

namespace FeatherWings.LedMatrix8x16_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        LedMatrix8x16Wing ledMatrixWing;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();

            PixelWalk();

            FourCorners();
       
            Thread.Sleep(2000);

            graphics.Rotation = GraphicsLibrary.RotationType._90Degrees;
            graphics.Clear();
            graphics.DrawText(0, 0, "M F7");
            graphics.Show();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            ledMatrixWing = new LedMatrix8x16Wing(Device.CreateI2cBus());
            ledMatrixWing.Clear();
   
            graphics = new GraphicsLibrary(ledMatrixWing);
            graphics.CurrentFont = new Font4x8();
        }

        void PixelWalk()
        {
            for (byte j = 0; j < 16; j++)
            {
                for (byte i = 0; i < 8; i++)
                {
                    ledMatrixWing.Clear();
                    ledMatrixWing.DrawPixel(i, j);
                    ledMatrixWing.Show();
                    Thread.Sleep(50);
                }
            }
        }

        void FourCorners()
        {
            ledMatrixWing.Clear();
            ledMatrixWing.DrawPixel(0, 0);
            ledMatrixWing.DrawPixel(7, 0);
            ledMatrixWing.DrawPixel(0, 7);
            ledMatrixWing.DrawPixel(7, 7);
            ledMatrixWing.Show();
        }
    }
}