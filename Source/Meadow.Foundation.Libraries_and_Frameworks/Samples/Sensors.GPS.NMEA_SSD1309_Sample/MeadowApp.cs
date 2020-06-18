using System;
using System.Text;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Communications;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Hardware;

namespace Sensors.GPS.NMEA_SSD1309_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        SerialTextFile serialTextFile;
        NmeaSentenceParser nmea;
        SerialMessagePort port;
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
            port = Device.CreateSerialMessagePort(
                Device.SerialPortNames.Com4,
                suffixDelimiter: Encoding.ASCII.GetBytes("\n"),
                preserveDelimiter: true,
                9600);
            Console.WriteLine("Serial port created");

            port.MessageReceived += (object sender, SerialMessageEventArgs e) => {
                nmea.ParseNmeaMessage(e.GetMessageString(Encoding.ASCII));
            };

            nmea = new NmeaSentenceParser();
            var ggaParser = new GgaParser();
            ggaParser.OnPositionReceived += GgaParser_OnPositionReceived;
            nmea.AddParser(ggaParser);

            // open serial
            port.Open();
        }


        private void GgaParser_OnPositionReceived(object sender, Meadow.Peripherals.Sensors.Location.Gnss.GnssPositionInfo location)
        {
            port.Close();
            graphics.Clear();
            graphics.DrawText(0, 0, "Latitude:");
            graphics.DrawText(0, 11, $"{location.Position.Latitude?.Degrees} {location.Position.Latitude?.Direction}");
            graphics.DrawText(0, 22, "Longitude:");
            graphics.DrawText(0, 33,$"{location.Position.Longitude?.Degrees} {location.Position.Longitude?.Direction}");
            graphics.DrawText(0, 44, "Altitude:");
            graphics.DrawText(0, 55, $"{location.Position.Altitude}m");
            graphics.Show();
            port.Open();
        }
    }
}