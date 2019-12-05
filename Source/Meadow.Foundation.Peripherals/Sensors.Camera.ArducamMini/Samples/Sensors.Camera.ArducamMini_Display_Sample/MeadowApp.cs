using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Foundation.Displays.Tft;
using Meadow.Hardware;
using Meadow.Foundation.Graphics;
using NanoJpeg;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ArducamMini camera;

        ST7789 display;

        GraphicsLibrary graphics;
        

        public MeadowApp()
        {
            Initialize();

         /*   Console.WriteLine("Draw text");
            graphics.Clear();
            graphics.DrawText(0, 0, "Camera sample", Meadow.Foundation.Color.AliceBlue);
            Console.WriteLine("Show text");
            graphics.Show();
            Console.WriteLine("Draw complete"); */

            var data = CaptureImage();

            JpegTest(data);
        }

        void JpegTest(byte[] data)
        {
            var nanoJpeg = new NanoJPEG();

            nanoJpeg.njDecode(data);

            Console.WriteLine("Jpg decoded");

            var jpg = nanoJpeg.GetImage();

            Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes");
            Console.WriteLine($"Width {nanoJpeg.Width}");
            Console.WriteLine($"Height {nanoJpeg.Height}");

            graphics.Clear();

            int x = 0;
            int y = 0;
            byte r, g, b;

            for (int i = 0; i < jpg.Length; i += 3)
            {
                r = jpg[i];
                g = jpg[i + 1];
                b = jpg[i + 2];

                display.DrawPixel(x, y, r, g, b);

                x++;

                if (x % 240 == 0)
                {
                    y++;
                    x = 0;
                }

                if(y >= 135)
                {
                    break;
                }
            }

            Console.WriteLine("Jpeg show");

            display.Show();
        }

        void Initialize()
        {
            Console.WriteLine("Creating output ports...");

            var spiBus = Device.CreateSpiBus();

            camera = new ArducamMini(Device, spiBus, Device.Pins.D00, Device.CreateI2cBus());

        //    display = new ST7789(Device, spiBus,
        //        Device.Pins.D04, Device.Pins.D03, Device.Pins.D02, 135, 240);

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font12x20();
            graphics.Rotation = GraphicsLibrary.RotationType._90Degrees;

        }

        byte[] CaptureImage()
        { 
            Thread.Sleep(200);

            Console.WriteLine("Attempting single capture");
            camera.FlushFifo();
            camera.ClearFifoFlag();
            camera.StartCapture();

            Console.WriteLine("Capture started");

            byte[] data = null;

            Thread.Sleep(1000);

            if (camera.IsCaptureComplete())
            {
                Console.WriteLine("Capture complete");

                data = camera.GetImageData();

                Console.WriteLine($"Jpeg captured {data.Length}");
            }

            return data;
        }
    }
}