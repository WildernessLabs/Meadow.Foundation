using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
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
        ISerialMessagePort serialPort;
        NmeaSentenceProcessor nmeaProcessor;
        Stopwatch uptime;
        RgbPwmLed heartbeatLed;

        public MeadowApp()
        {
            Console.WriteLine($"Start the SerialGPS_Listener app.");
            Initialize();
            Start();
        }

        void Initialize()
        {
            uptime = new Stopwatch();
            uptime.Start();

            serialPort = Device.CreateSerialMessagePort(
                Device.SerialPortNames.Com4,
                suffixDelimiter: Encoding.UTF8.GetBytes("\r\n"),
                preserveDelimiter: true);
            serialPort.MessageReceived += SerialPort_MessageReceived;

            InitDecoders();

            heartbeatLed = new RgbPwmLed(
                Device, Device.Pins.OnboardLedRed, Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                commonType: Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);
        }

        void Start()
        {
            Console.WriteLine("Starting.");
            HeartBeat();
            serialPort.Open();
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            Console.WriteLine("Message received.");
            
            Console.WriteLine($"{e.GetMessageString(Encoding.UTF8)}");
            nmeaProcessor.ProcessNmeaMessage(e.GetMessageString(Encoding.UTF8));
        }

        protected void InitDecoders()
        {
            Console.WriteLine("Create NMEA");
            nmeaProcessor = new NmeaSentenceProcessor();
            // verbose output
            nmeaProcessor.DebugMode = true;

            Console.WriteLine("Add decoders");

            // GGA
            var ggaDecoder = new GgaDecoder();
            Console.WriteLine("Created GGA");
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };

            // GLL
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(activeSatellites);
                Console.WriteLine("*********************************************");
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(positionCourseAndTime);
                Console.WriteLine("*********************************************");

            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{courseAndVelocity}");
                Console.WriteLine("*********************************************");
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{satellites}");
                Console.WriteLine("*********************************************");
            };

        }

        void HeartBeat() {
            new Thread(() => {
                while (true) {
                    Console.WriteLine($"Uptime: {uptime.Elapsed}");
                    heartbeatLed.SetColor(Color.FromHex("#23abe3"));
                    Thread.Sleep(500);
                    heartbeatLed.IsOn = false;
                    Thread.Sleep(10000); //10 seconds
                }
            }).Start();
        }
    }
}