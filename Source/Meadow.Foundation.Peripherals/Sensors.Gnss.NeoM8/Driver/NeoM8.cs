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

        CommunicationMode communicationMode;

        SerialMessageProcessor messageProcessor;

        const byte BUFFER_SIZE = 128;
        const byte COMMS_SLEEP_MS = 200;

        /// <summary>
        /// Reset the device
        /// </summary>
        public async Task Reset()
        {
            if(ResetPort != null)
            {
                ResetPort.State = false;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                ResetPort.State = true;
            }
        }

        /// <summary>
        /// Start updating
        /// </summary>
        public void StartUpdating()
        {
            switch(communicationMode)
            {
                case CommunicationMode.Serial:
                    StartUpdatingSerial();
                    break;
                case CommunicationMode.SPI:
                    _ = StartUpdatingSpi();
                    break;
                case CommunicationMode.I2C:
                    _ = StartUpdatingI2c();
                    break;
            }
        }

        void InitDecoders()
        {
            nmeaProcessor = new NmeaSentenceProcessor();

            var ggaDecoder = new GgaDecoder();
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

        void MessageReceived(object sender, SerialMessageData e)
        {
            string msg = e.GetMessageString(Encoding.ASCII);

            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}