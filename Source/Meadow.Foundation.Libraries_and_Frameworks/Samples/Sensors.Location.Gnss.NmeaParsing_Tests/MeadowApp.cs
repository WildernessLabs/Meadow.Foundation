using System;
using System.Collections.Generic;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        List<string> sentences;
        NmeaSentenceProcessor nmeaProcessor;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize");
            this.sentences = GetSampleNmeaSentences();

            InitDecoders();

            foreach (string sentence in sentences) {
                Console.WriteLine($"About to process:{sentence}");
                nmeaProcessor.ProcessNmeaMessage(sentence);
            }

            Console.WriteLine("Made it through all sentences");
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

        public List<string> GetSampleNmeaSentences()
        {
            List<string> sentences = new List<string>() {
                "$GPGGA,000049.799,,,,,0,00,,,M,,M,,*72", // i think not valid.
                "$GNGGA,001043.00,4404.14036,N,12118.85961,W,1,12,0.98,1113.0,M,-21.3,M,,*47", // valid
                "$GNRMC,001031.00,A,4404.13993,N,12118.86023,W,0.146,,100117,,,A*7B", // valid
                "$GPVTG,220.86,T,,M,2.550,N,4.724,K,A*34", // Valid
                "$GPVTG,0.00,T,,M,0.00,N,0.00,K,N*32",
                "$GPRMC,162254.00,A,3723.02837,N,12159.39853,W,0.820,188.36,110706,,,A*74",
                "$GPVTG,188.36,T,,M,0.820,N,1.519,K,A*3F",
                "$GPGGA,162254.00,3723.02837,N,12159.39853,W,1,03,2.36,525.6,M,-25.6,M,,*65",
                "$GPGSA,A,2,25,01,22,,,,,,,,,,2.56,2.36,1.00*02",

                "$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74",
                "$GPGSV,3,2,11,14,25,170,00,16,57,208,39,18,67,296,40,19,40,246,00*74",
                "$GPGSV,3,3,11,22,42,067,42,24,14,311,43,27,05,244,00,,,,*4D",

                "$GPGLL,3723.02837,N,12159.39853,W,162254.00,A,A*7C",
                "$GPZDA,162254.00,11,07,2006,00,00*63",
                "$GNGLL,,,,,,V,N*7A",
                "$GLGSV,1,1,00*65",
            };

            return sentences;

        }
    }
}