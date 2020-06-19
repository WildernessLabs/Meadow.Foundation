﻿using System;
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
        NmeaSentenceProcessor nmeaParser;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize");
            this.sentences = GetSampleNmeaSentences();

            InitParsers();

            foreach (string sentence in sentences) {
                Console.WriteLine($"About to parse:{sentence}");
                nmeaParser.ParseNmeaMessage(sentence);
            }

            Console.WriteLine("Made it through all sentences");
        }

        protected void InitParsers()
        {
            Console.WriteLine("Create NMEA");
            nmeaParser = new NmeaSentenceProcessor();

            Console.WriteLine("Add parsers");

            // GGA
            var ggaParser = new GgaParser();
            Console.WriteLine("Created GGA");
            nmeaParser.RegisterParser(ggaParser);
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

        }

        public List<string> GetSampleNmeaSentences()
        {
            List<string> sentences = new List<string>() {
                "$GPGGA,000049.799,,,,,0,00,,,M,,M,,*72",
                "$GPGSA,A,1,,,,,,,,,,,,,,,*1E",
                "$GPRMC,000049.799,V,,,,,0.00,0.00,060180,,,N*48",
                "$GPVTG,0.00,T,,M,0.00,N,0.00,K,N*32",
                "$GPRMC012539.800,V,,,,,0.00,0.00,060180,,,N*78",
                "$GPRMC,162254.00,A,3723.02837,N,12159.39853,W,0.820,188.36,110706,,,A*74",
                "$GPVTG,188.36,T,,M,0.820,N,1.519,K,A*3F",
                "$GPGGA,162254.00,3723.02837,N,12159.39853,W,1,03,2.36,525.6,M,-25.6,M,,*65",
                "$GPGSA,A,2,25,01,22,,,,,,,,,,2.56,2.36,1.00*02",

                "$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74",
                "$GPGSV,3,2,11,14,25,170,00,16,57,208,39,18,67,296,40,19,40,246,00*74",
                "$GPGSV,3,3,11,22,42,067,42,24,14,311,43,27,05,244,00,,,,*4D",

                //"$GPGSV,4,2,14,18,16,079,,11,19,312,,14,80,041,,21,04,135,25*7D",
                //"$GPGSV,4,3,14,15,27,134,18,03,25,222,,22,51,057,16,09,07,036,*79",
                //"$GPGSV,4,4,14,07,01,181,,15,25,135,*76",
                "$GPGLL,3723.02837,N,12159.39853,W,162254.00,A,A*7C",
                "$GPZDA,162254.00,11,07,2006,00,00*63",
                "$GNGLL,,,,,,V,N*7A",
                "$GLGSV,1,1,00*65",
            };

            return sentences;

        }
    }
}