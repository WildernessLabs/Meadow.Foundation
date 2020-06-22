using System;
using System.Collections.Generic;
using System.Text;
using Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Sensors.Location.MediaTek
{
    public class NmeaEventArgs
    {
        public string NmeaSentence { get; set; }
    }

    public class Mt3339
    {
        public int BaudRate {
            get => serialPort.BaudRate;
            set => serialPort.BaudRate = value;
        }

        ISerialMessagePort serialPort;
        NmeaSentenceProcessor nmeaProcessor;

        public event EventHandler<NmeaEventArgs> NmeaSentenceArrived = delegate { };

        public Mt3339(ISerialMessagePort serialPort, int baud = 9600)
        {
            this.serialPort = serialPort;

            BaudRate = baud;
            Init();
        }

        public Mt3339(
            IIODevice device,
            SerialPortName serialPortName,
            byte[] suffixDelimiter,
            bool preserveDelimiter,
            int baud = 9600,
            int dataBits = 8,
            Parity partity = Parity.None,
            StopBits stopBits = StopBits.One,
            int readBufferSize = 4096) :
            this(device.CreateSerialMessagePort(
                serialPortName,
                suffixDelimiter,
                preserveDelimiter,
                baud, dataBits, partity, stopBits, readBufferSize), baud)
        { }

        protected void Init()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();
        }

        public void StartUpdataing()
        {
            Console.WriteLine("initializing serial port");
            serialPort.Open();
            Console.WriteLine("serial port opened.");
        }

        protected void InitDecoders()
        {
            Console.WriteLine("Create NMEA");
            nmeaProcessor = new NmeaSentenceProcessor();

            Console.WriteLine("Add decoders");

            // GGA
            var ggaDecoder = new GgaDecoder();
            Console.WriteLine("Created GGA");
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("Location information received.");
                Console.WriteLine($"Talker ID: {location.TalkerID}, talker name: {location.TalkerSystemName}");
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
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("GLL information received.");
                Console.WriteLine($"Talker ID: {location.TalkerID}, talker name: {location.TalkerSystemName}");
                Console.WriteLine($"Time of reading: {location.TimeOfReading}");
                Console.WriteLine($"Latitude: {location.Position.Latitude}");
                Console.WriteLine($"Longitude: {location.Position.Longitude}");
                Console.WriteLine("*********************************************");
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                Console.WriteLine("Satellite (GSA) information received.");
                Console.WriteLine($"Talker ID: {activeSatellites.TalkerID}, talker name: {activeSatellites.TalkerSystemName}");
                Console.WriteLine($"Number of satellites involved in fix: {activeSatellites.SatellitesUsedForFix?.Length}");
                Console.WriteLine($"Dilution of precision: {activeSatellites.DilutionOfPrecision:f2}");
                Console.WriteLine($"HDOP: {activeSatellites.HorizontalDilutionOfPrecision:f2}");
                Console.WriteLine($"VDOP: {activeSatellites.VerticalDilutionOfPrecision:f2}");
                Console.WriteLine("*********************************************");
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                Console.WriteLine("Recommended Minimum sentence \"C\" (RMC) received.");
                Console.WriteLine($"Talker ID: {positionCourseAndTime.TalkerID}, talker name: {positionCourseAndTime.TalkerSystemName}");
                Console.WriteLine($"Time of reading: {positionCourseAndTime.TimeOfReading}");
                Console.WriteLine($"Latitude: {positionCourseAndTime.Position.Latitude}");
                Console.WriteLine($"Longitude: {positionCourseAndTime.Position.Longitude}");
                Console.WriteLine($"Speed: {positionCourseAndTime.SpeedInKnots:f2}");
                Console.WriteLine($"Course: {positionCourseAndTime.CourseHeading:f2}");
                Console.WriteLine("*********************************************");

            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Console.WriteLine("Course made good (VTG) received.");
                Console.WriteLine($"Talker ID: {courseAndVelocity.TalkerID}, talker name: {courseAndVelocity.TalkerSystemName}");
                Console.WriteLine($"True heading: {courseAndVelocity.TrueHeading:f2}");
                Console.WriteLine($"Magnetic heading: {courseAndVelocity.MagneticHeading:f2}");
                Console.WriteLine($"Knots: {courseAndVelocity.Knots:f2}");
                Console.WriteLine($"KPH: {courseAndVelocity.Kph:f2}");
                Console.WriteLine("*********************************************");
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
                Console.WriteLine($"Satellites in view (GSA) received, count: {satellites.Satellites.Length}");
                Console.WriteLine($"Talker ID: {satellites.TalkerID}, talker name: {satellites.TalkerSystemName}");
                foreach (var sat in satellites.Satellites) {
                    Console.WriteLine("---------------");
                    Console.WriteLine($"ID: {sat.ID}");
                    Console.WriteLine($"Azimuth: {sat.Azimuth}");
                    Console.WriteLine($"Elevation: {sat.Elevation}");
                    Console.WriteLine($"Signal to Noise Ratio: {sat.SignalTolNoiseRatio}");
                }
                Console.WriteLine("*********************************************");
            };

        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            Console.WriteLine("Message arrived.");

            string msg = (e.GetMessageString(Encoding.ASCII));

            Console.WriteLine($"msg:{msg}");

            Console.WriteLine($"Sending off to the parser");
            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}