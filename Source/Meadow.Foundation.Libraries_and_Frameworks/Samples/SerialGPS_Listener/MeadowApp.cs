using System;
using System.Text;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace MeadowApp
{
    /// <summary>
    /// A simple app that listens to a serial GPS for NMEA sentences, parses
    /// them, and writes them to the console.
    /// </summary>
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        SerialMessagePort serialPort;
        NmeaSentenceProcessor nmeaParser;

        public MeadowApp()
        {
            Console.WriteLine($"Start the SerialGPS_Listener app.");
            Initialize();
            Start();
        }

        void Initialize()
        {
            serialPort = Device.CreateSerialMessagePort(
                Device.SerialPortNames.Com4,
                suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true,
                9600);
            serialPort.MessageReceived += SerialPort_MessageReceived;

            InitParsers();
        }

        void Start()
        {
            Console.WriteLine("Starting.");
            serialPort.Open();
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageEventArgs e)
        {
            Console.WriteLine("Message received.");
            Console.WriteLine($"[{e.GetMessageString(Encoding.ASCII)}]");
            nmeaParser.ParseNmeaMessage(e.GetMessageString(Encoding.ASCII));
        }

        protected void InitParsers()
        {
            Console.WriteLine("Create NMEA");
            nmeaParser = new NmeaSentenceProcessor();

            Console.WriteLine("Add parsers");

            // GGA
            var ggaParser = new GgaParser();
            nmeaParser.RegisterParser(/*wtf C#*/ggaParser);
            ggaParser.PositionReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine($"location.Valid:{location.Valid}");
                Console.WriteLine($"location.NumberOfSatellites:{location.NumberOfSatellites}");
                Console.WriteLine($"location.Position.Latittude:{location.Position.Latitude}");

                Console.WriteLine("Location information received.");
                Console.WriteLine($"Time of reading: {location.TimeOfReading}");
                Console.WriteLine($"Valid: {location.Valid}");
                Console.WriteLine($"Latitude: {location.Position.Latitude}");
                Console.WriteLine($"Longitude: {location.Position.Longitude}");
                Console.WriteLine($"Altitude: {location.Position.Altitude:f2}");
                Console.WriteLine($"Number of satellites: {location.NumberOfSatellites}");
                Console.WriteLine($"Fix quality: {location.FixQuality}");
                Console.WriteLine($"HDOP: {location.HorizontalDilutionOfPrecision:f2}");
                Console.WriteLine("*********************************************");
            };

            // GLL
            var gllParser = new GllParser();
            nmeaParser.RegisterParser(gllParser);
            gllParser.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("GLL information received.");
                Console.WriteLine($"Time of reading: {location.TimeOfReading}");
                Console.WriteLine($"Latitude: {location.Position.Latitude}");
                Console.WriteLine($"Longitude: {location.Position.Longitude}");
                Console.WriteLine("*********************************************");
            };

            // GSA
            var gsaParser = new GsaParser();
            nmeaParser.RegisterParser(gsaParser);
            gsaParser.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                Console.WriteLine("Satellite (GSA) information received.");
                Console.WriteLine($"Number of satellites involved in fix: {activeSatellites.SatellitesUsedForFix?.Length}");
                Console.WriteLine($"Dilution of precision: {activeSatellites.DilutionOfPrecision:f2}");
                Console.WriteLine($"HDOP: {activeSatellites.HorizontalDilutionOfPrecision:f2}");
                Console.WriteLine($"VDOP: {activeSatellites.VerticalDilutionOfPrecision:f2}");
                Console.WriteLine("*********************************************");
            };

            // RMC (recommended minimum)
            var rmcParser = new RmcParser();
            nmeaParser.RegisterParser(rmcParser);
            rmcParser.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                //Console.WriteLine($"RMC message decoded; time:{positionCourseAndTime.TimeOfReading}UTC, valid:{positionCourseAndTime.Valid}");
                //if (positionCourseAndTime.Valid) {
                //    Console.WriteLine($"lat:{positionCourseAndTime.Position.Latitude}, long: {positionCourseAndTime.Position.Longitude}");
                //}
                //Console.WriteLine("I wish a muthafucka would.");

                Console.WriteLine("Recommended Minimum sentence \"C\" (RMC) received.");
                Console.WriteLine($"Time of reading: {positionCourseAndTime.TimeOfReading}");
                Console.WriteLine($"Latitude: {positionCourseAndTime.Position.Latitude}");
                Console.WriteLine($"Longitude: {positionCourseAndTime.Position.Longitude}");
                Console.WriteLine($"Speed: {positionCourseAndTime.SpeedInKnots:f2}");
                Console.WriteLine($"Course: {positionCourseAndTime.CourseHeading:f2}");
                Console.WriteLine("*********************************************");

            };

            // VTG (course made good)
            var vtgParser = new VtgParser();
            nmeaParser.RegisterParser(vtgParser);
            vtgParser.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Console.WriteLine("Course made good (VTG) received.");
                Console.WriteLine($"True heading: {courseAndVelocity.TrueHeading:f2}");
                Console.WriteLine($"Magnetic heading: {courseAndVelocity.MagneticHeading:f2}");
                Console.WriteLine($"Knots: {courseAndVelocity.Knots:f2}");
                Console.WriteLine($"KPH: {courseAndVelocity.Kph:f2}");
                Console.WriteLine("*********************************************");
            };

            // GSV (satellites in view)
            var gsvParser = new GsvParser();
            nmeaParser.RegisterParser(gsvParser);
            gsvParser.SatellitesInViewReceived += (object sender, List<Satellite> satellites) => {
                Console.WriteLine($"Satellites in view (GSA) received, count: {satellites.Count}");
                foreach (var sat in satellites) {
                    Console.WriteLine("---------------");
                    Console.WriteLine($"ID: {sat.ID}");
                    Console.WriteLine($"Azimuth: {sat.Azimuth}");
                    Console.WriteLine($"Elevation: {sat.Elevation}");
                    Console.WriteLine($"Signal to Noise Ratio: {sat.SignalTolNoiseRatio}");
                }
                Console.WriteLine("*********************************************");
            };

            Console.WriteLine("Parsers configured.");
        }


    }
}