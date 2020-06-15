using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Communications;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.GPS;
using Meadow.Hardware;

namespace Sensors.GPS.NMEA_SSD1309_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        SerialTextFile serialTextFile;
        NMEAMessageDecoder nmea;
        ISerialPort port;
        byte[] data = new byte[512];

        Ssd1309 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            display = new Ssd1309(Device.CreateI2cBus());
            Console.WriteLine("Display created");

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font8x8();
            Console.WriteLine("Graphics library created");

            //COM4 - Pins D00 & D01 on the Meadow F7
            port = Device.CreateSerialPort(Device.SerialPortNames.Com4, 9600);
            Console.WriteLine("Serial port created");
            port.Open();

            nmea = new NMEAMessageDecoder();
            var ggaDecoder = new GGADecoder();
            ggaDecoder.OnPositionReceived += GgaDecoder_OnPositionReceived;
            nmea.AddDecoder(ggaDecoder);

            serialTextFile = new SerialTextFile(port, "\r\n");
            serialTextFile.OnLineReceived += SerialTextFile_OnLineReceived;
        }

        private void SerialTextFile_OnLineReceived(object sender, string line)
        {
            nmea.SetNmeaMessage(line);
        }

        private void GgaDecoder_OnPositionReceived(object sender, GPSLocation location)
        {
            port.Close();
            graphics.Clear();
            graphics.DrawText(0, 0, "Latitude:");
            graphics.DrawText(0, 11, $"{location.Latitude.Degrees} {location.Latitude.Direction}");
            graphics.DrawText(0, 22, "Longitude:");
            graphics.DrawText(0, 33,$"{location.Longitude.Degrees} {location.Longitude.Direction}");
            graphics.DrawText(0, 44, "Altitude:");
            graphics.DrawText(0, 55, $"{location.Altitude}m");
            graphics.Show();
            port.Open();
        }
    }
}