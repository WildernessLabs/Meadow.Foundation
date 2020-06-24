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
        //public int BaudRate {
        //    get => serialPort.BaudRate;
        //    set => serialPort.BaudRate = value;
        //}

        ISerialMessagePort serialPort;
        NmeaSentenceProcessor nmeaProcessor;

        //public event EventHandler<NmeaEventArgs> NmeaSentenceArrived = delegate { };

        public event EventHandler<GnssPositionInfo> GgaReceived = delegate { };
        public event EventHandler<GnssPositionInfo> GllReceived = delegate { };
        public event EventHandler<ActiveSatellites> GsaReceived = delegate { };
        public event EventHandler<GnssPositionInfo> RmcReceived = delegate { };
        public event EventHandler<CourseOverGround> VtgReceived = delegate { };
        public event EventHandler<SatellitesInView> GsvReceived = delegate { };


        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        protected Mt3339(ISerialMessagePort serialPort)
        {
            this.serialPort = serialPort;

            this.serialPort.MessageReceived += SerialPort_MessageReceived;

            Init();
        }

        public Mt3339( IIODevice device, SerialPortName serialPortName)
            : this(device.CreateSerialMessagePort(
                serialPortName, suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true, readBufferSize: 512))
        { }

        protected void Init()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();
            Console.WriteLine("Finish Mt3339 initialization.");
        }

        public void StartUpdataing()
        {
            // open the serial connection
            serialPort.Open();
            Console.WriteLine("serial port opened.");

            //==== setup commands

            // get release and version
            Console.WriteLine("Asking for release and version.");
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));

            // get atntenna info
            Console.WriteLine("Start output antenna info");
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));

            // turn on all data
            Console.WriteLine("Turning on all data");
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
        }

        protected void InitDecoders()
        {
            Console.WriteLine("Create NMEA");
            nmeaProcessor = new NmeaSentenceProcessor();

            Console.WriteLine("Add decoders");

            // MTK
            var mtkDecoder = new MtkDecoder();
            Console.WriteLine("Created MTK");
            nmeaProcessor.RegisterDecoder(mtkDecoder);
            mtkDecoder.MessageReceived += (object sender, string message) => {
                Console.WriteLine($"MTK Message:{message}");
            };

            // GGA
            var ggaDecoder = new GgaDecoder();
            Console.WriteLine("Created GGA");
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => {
                this.GgaReceived(this, location);
            };

            // GLL
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                this.GllReceived(this, location);
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                this.GsaReceived(this, activeSatellites);
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                this.RmcReceived(this, positionCourseAndTime);
            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                this.VtgReceived(this, courseAndVelocity);
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
                this.GsvReceived(this, satellites);
            };

        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = (e.GetMessageString(Encoding.ASCII));

            Console.WriteLine($"Message arrived:{msg}");

            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}