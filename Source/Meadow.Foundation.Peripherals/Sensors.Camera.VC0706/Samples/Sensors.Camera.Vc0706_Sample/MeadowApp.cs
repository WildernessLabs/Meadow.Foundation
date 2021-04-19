using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;
using SimpleJpegDecoder;

namespace Sensors.Camera.Vc0706_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Vc0706 camera;

        GraphicsLibrary graphics;
        Ili9341 display;

        RgbPwmLed onboardLed;

        public MeadowApp()
        {
            Initialize();

            CameraTest();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            Console.WriteLine("Init camera");

            camera = new Vc0706(Device, Device.SerialPortNames.Com4, 38400);

            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(24000, SpiClockConfiguration.Mode.Mode0);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new Ili9341
            (
                device: Device,
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D13,
                dcPin: Device.Pins.D14,
                resetPin: Device.Pins.D15,
                width: 240, height: 320
            );

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font12x20();

            Console.WriteLine("Init complete");
        }

        void CameraTest()
        {
            Console.WriteLine("Set image size");
            camera.SetImageSize(Vc0706.ImageSize._320x240);

            Console.WriteLine($"Image size is {camera.GetImageSize()}");

            Console.WriteLine("Set TV off");
            camera.TvOff();
            Thread.Sleep(2000);
            Console.WriteLine("Set TV on");
            camera.TvOn();

            Thread.Sleep(2000);

            Console.WriteLine(camera.GetVersion());

            Thread.Sleep(2000);

            camera.SetOnScreenDisplay(10, 10, "Hi");

            Thread.Sleep(2000);

            Console.WriteLine("Take picture");
            camera.TakePicture();

            uint frameLen = camera.GetFrameLength();
            Console.WriteLine($"Frame length: {frameLen}");

            byte bytesToRead;
            byte[] jpg;

            var decoder = new JpegDecoder();

            using (var stream = new MemoryStream())
            {
                Console.WriteLine($"Decode jpeg - this operation may take serveral seconds");

                while (frameLen > 0)
                {
                    bytesToRead = (byte)Math.Min(32, frameLen);

                    var buffer = camera.ReadPicture(bytesToRead);

                    stream.Write(buffer, 0, bytesToRead);
                    frameLen -= bytesToRead;
                }
                jpg = decoder.DecodeJpeg(stream.ToArray());
            }

            Console.WriteLine($"Jpeg data length: {jpg.Length}");

            Console.WriteLine($"Jpeg decoded is {decoder.ImageSize} bytes");
            Console.WriteLine($"Width {decoder.Width}");
            Console.WriteLine($"Height {decoder.Height}");
            Console.WriteLine($"IsColor {decoder.IsColor}");

            graphics.Clear();
            graphics.DrawRectangle(0, 0, 240, 320, Color.White, true);

            int x = 0;
            int y = 0;
            //int y = (320 - image.Height) / 2;
            byte r, g, b;

            for (int i = 0; i < jpg.Length; i += 3)
            {
                r = jpg[i];
                g = jpg[i + 1];
                b = jpg[i + 2];

                display.DrawPixel(y, x, r, g, b);

                x++;

                if (x % decoder.Width == 0)
                {
                    y++;
                    x = 0;
                }
            }

            Console.WriteLine("Jpeg show");

            camera.TvOn();

            display.Show();
        }
    }
}