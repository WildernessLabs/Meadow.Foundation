using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
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

        public IDigitalInputPort? PpsPort { get; }
        protected IDigitalOutputPort ResetPort { get; }

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPort"></param>
        protected NeoM8(ISerialMessagePort serialPort, IDigitalOutputPort reset, IDigitalInputPort? pps = null)
        {
            this.serialPort = serialPort;
            ResetPort = reset;
            PpsPort = pps;

            Init();
        }

        /// <summary>
        /// Create a new Mt3339 object
        /// </summary>
        /// <param name="device">IMeadowDevice instance</param>
        /// <param name="serialPortName">The serial port name to create</param>
        public NeoM8(IMeadowDevice device, SerialPortName serialPortName, IPin resetPin, IPin? ppsPin = null)
            : this(device.CreateSerialMessagePort(
                serialPortName, suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true, readBufferSize: 512),
                device.CreateDigitalOutputPort(resetPin, true), // initialize high to enable the receiver
                device.CreateDigitalInputPort(ppsPin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        { }

        protected void Init()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();

            Reset().Wait();

            Resolver.Log.Debug("Finish NeoM8 initialization.");
        }

        public async Task Reset()
        {
            ResetPort.State = false;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            ResetPort.State = true;
        }

        public void StartUpdating()
        {
            if (serialPort.IsOpen)
            {
                Resolver.Log.Debug("serial port already open.");
                return;
            }

            // open the serial connection
            Resolver.Log.Debug("opening serial port.");
            serialPort.Open();
            Resolver.Log.Debug("serial port opened.");

            //==== setup commands

            Resolver.Log.Debug("Requesting NMEA data");
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));
            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));
        }

        protected void InitDecoders()
        {
            Resolver.Log.Debug("Create NMEA");
            nmeaProcessor = new NmeaSentenceProcessor();

            Resolver.Log.Debug("Add decoders");

            // GGA
            var ggaDecoder = new GgaDecoder();
            Resolver.Log.Debug("Created GGA");
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) =>
            {
                GgaReceived(this, location);
            };

            // GLL
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) =>
            {
                GllReceived(this, location);
            };

            // GSA
            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                GsaReceived(this, activeSatellites);
            };

            // RMC (recommended minimum)
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                RmcReceived(this, positionCourseAndTime);
            };

            // VTG (course made good)
            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                VtgReceived(this, courseAndVelocity);
            };

            // GSV (satellites in view)
            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) =>
            {
                GsvReceived(this, satellites);
            };
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = (e.GetMessageString(Encoding.ASCII));

            Resolver.Log.Debug($"Message arrived:{msg}");

            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}