using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// Represents a NEO-M8 GNSS module
    /// </summary>
    public partial class NeoM8
    {
        readonly ISerialMessagePort serialPort;
        NmeaSentenceProcessor nmeaProcessor;

        /// <summary>
        /// Raised when GGA position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> GgaReceived = delegate { };

        /// <summary>
        /// Raised when GLL position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> GllReceived = delegate { };

        /// <summary>
        /// Raised when GSA satellite data is received
        /// </summary>
        public event EventHandler<ActiveSatellites> GsaReceived = delegate { };

        /// <summary>
        /// Raised when RMC position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> RmcReceived = delegate { };

        /// <summary>
        /// Raised when VTG course over ground data is received
        /// </summary>
        public event EventHandler<CourseOverGround> VtgReceived = delegate { };

        /// <summary>
        /// Raised when GSV satellite data is received
        /// </summary>
        public event EventHandler<SatellitesInView> GsvReceived = delegate { };

        /// <summary>
        /// NeoM8 pulse per second port
        /// </summary>
        public IDigitalInputPort PulsePerSecondPort { get; }

        /// <summary>
        /// NeoM8 reset port
        /// Initialize high to enable the device
        /// </summary>
        protected IDigitalOutputPort ResetPort { get; }

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        /// <summary>
        /// Create a new NEOM8 object
        /// </summary>
        protected NeoM8(ISerialMessagePort serialPort, IDigitalOutputPort reset, IDigitalInputPort pps = null)
        {
            this.serialPort = serialPort;
            ResetPort = reset;
            PulsePerSecondPort = pps;

            Initialize();
        }

        /// <summary>
        /// Create a new NEOM8 object
        /// </summary>
        /// <param name="device">IMeadowDevice instance</param>
        /// <param name="serialPortName">The serial port name to create</param>
        /// <param name="resetPin">The reset pin</param>
        /// <param name="ppsPin">The pulse per second pin</param>
        public NeoM8(IMeadowDevice device, SerialPortName serialPortName, IPin resetPin, IPin ppsPin = null)
            : this(device.CreateSerialMessagePort(
                serialPortName, suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true, readBufferSize: 512),
                device.CreateDigitalOutputPort(resetPin, true),
                device.CreateDigitalInputPort(ppsPin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        { }

        void Initialize()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();

            Reset().Wait();

            Resolver.Log.Debug("Finish NeoM8 initialization.");
        }

        /// <summary>
        /// Reset the device
        /// </summary>
        public async Task Reset()
        {
            ResetPort.State = false;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            ResetPort.State = true;
        }

        /// <summary>
        /// Start updating
        /// </summary>
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

            Resolver.Log.Debug("Requesting NMEA data");
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));
        }

        void InitDecoders()
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

            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) =>
            {
                GllReceived(this, location);
            };

            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                GsaReceived(this, activeSatellites);
            };

            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                RmcReceived(this, positionCourseAndTime);
            };

            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                VtgReceived(this, courseAndVelocity);
            };

            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) =>
            {
                GsvReceived(this, satellites);
            };
        }

        void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = e.GetMessageString(Encoding.ASCII);

            Resolver.Log.Debug($"Message arrived:{msg}");

            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}