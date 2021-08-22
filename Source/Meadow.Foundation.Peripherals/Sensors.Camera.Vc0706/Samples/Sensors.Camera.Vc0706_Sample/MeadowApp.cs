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
        //<!—SNIP—>

        Vc0706 camera;

        //Uses SimpleJpegDecoder package for jpeg decoding
        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            camera = new Vc0706(Device, Device.SerialPortNames.Com4, 38400);

            camera.SetImageSize(Vc0706.ImageSize._320x240);
            Console.WriteLine($"Image size is {camera.GetImageSize()}");

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
        }

        //<!—SNOP—>
    }
}