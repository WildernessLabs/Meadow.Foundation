using System;
using System.Text;
using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// NMEA Event args - holds an NMEA sentance as a string
    /// </summary>
    public class NmeaEventArgs
    {
        /// <summary>
        /// The NMEA sentance
        /// </summary>
        public string NmeaSentence { get; set; } = string.Empty;
    }

    public partial class NeoM8
    {
        //public int BaudRate {
        //    get => serialPort.BaudRate;
        //    set => serialPort.BaudRate = value;
        //}

        readonly ISerialMessagePort serialPort;
        NmeaSentenceProcessor? nmeaProcessor;

        //public event EventHandler<NmeaEventArgs> NmeaSentenceArrived = delegate { };

        public event EventHandler<GnssPositionInfo> GgaReceived = delegate { };
        public event EventHandler<GnssPositionInfo> GllReceived = delegate { };
        public event EventHandler<ActiveSatellites> GsaReceived = delegate { };
        public event EventHandler<GnssPositionInfo> RmcReceived = delegate { };
        public event EventHandler<CourseOverGround> VtgReceived = delegate { };
        public event EventHandler<SatellitesInView> GsvReceived = delegate { };

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        protected NeoM8(ISerialMessagePort serialPort)
        {
            this.serialPort = serialPort;

            serialPort.MessageReceived += SerialPort_MessageReceived;

            Init();
        }

        /// <summary>
        /// Create a new Mt3339 object
        /// </summary>
        /// <param name="device">IMeadowDevice instance</param>
        /// <param name="serialPortName">The serial port name to create</param>
        public NeoM8(ISerialMessageController device, SerialPortName serialPortName)
            : this(device.CreateSerialMessagePort(
                serialPortName, suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true, readBufferSize: 512))
        { }

        protected void Init()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();
            Console.WriteLine("Finish NeoM8 initialization.");
        }

        public void StartUpdating()
        {
            // open the serial connection
            serialPort.Open();
            Console.WriteLine("serial port opened.");

            //==== setup commands

            //// get release and version
            //Console.WriteLine("Asking for release and version.");
            //this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));

            //// get atntenna info
            //Console.WriteLine("Start output antenna info");
            //this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));

            //// turn on all data
            //Console.WriteLine("Turning on all data");
            //this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
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
                GgaReceived(this, location);
            };

            // GLL
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                GllReceived(this, location);
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => {
                GsaReceived(this, activeSatellites);
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                RmcReceived(this, positionCourseAndTime);
            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                VtgReceived(this, courseAndVelocity);
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) => {
                GsvReceived(this, satellites);
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