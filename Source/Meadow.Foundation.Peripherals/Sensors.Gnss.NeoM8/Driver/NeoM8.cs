using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// Represents a NEO-M8 GNSS module
    /// </summary>
    public partial class NeoM8 : IGnssSensor
    {
        NmeaSentenceProcessor? nmeaProcessor;

        /// <summary>
        /// Raised when GNSS data is received
        /// </summary>
        public event EventHandler<IGnssResult> GnssDataReceived = default!;

        /// <summary>
        /// Supported GNSS result types
        /// </summary>
        public IGnssResult[] SupportedResultTypes { get; } = new IGnssResult[]
        {
            new GnssPositionInfo(),
            new ActiveSatellites(),
            new CourseOverGround()
        };

        /// <summary>
        /// Raised when GGA position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> GgaReceived = default!;

        /// <summary>
        /// Raised when GLL position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> GllReceived = default!;

        /// <summary>
        /// Raised when GSA satellite data is received
        /// </summary>
        public event EventHandler<ActiveSatellites> GsaReceived = default!;

        /// <summary>
        /// Raised when RMC position data is received
        /// </summary>
        public event EventHandler<GnssPositionInfo> RmcReceived = default!;

        /// <summary>
        /// Raised when VTG course over ground data is received
        /// </summary>
        public event EventHandler<CourseOverGround> VtgReceived = default!;

        /// <summary>
        /// Raised when GSV satellite data is received
        /// </summary>
        public event EventHandler<SatellitesInView> GsvReceived = default!;

        /// <summary>
        /// NeoM8 pulse per second port
        /// </summary>
        public IDigitalInputPort? PulsePerSecondPort { get; }

        /// <summary>
        /// NeoM8 reset port
        /// Initialize high to enable the device
        /// </summary>
        protected IDigitalOutputPort? ResetPort { get; }

        CommunicationMode communicationMode;

        SerialMessageProcessor? messageProcessor;

        CancellationTokenSource? cts;

        const byte BUFFER_SIZE = 128;
        const byte COMMS_SLEEP_MS = 200;

        /// <summary>
        /// Reset the device
        /// </summary>
        public async Task Reset()
        {
            if (ResetPort != null)
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
            switch (communicationMode)
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

        /// <summary>
        /// Stop updating
        /// </summary>
        public void StopUpdating()
        {
            switch (communicationMode)
            {
                case CommunicationMode.Serial:
                    StopUpdatingSerial();
                    break;
                case CommunicationMode.SPI:
                    StopUpdatingSpi();
                    break;
                case CommunicationMode.I2C:
                    StopUpdatingI2c();
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
                GgaReceived?.Invoke(this, location);
                GnssDataReceived?.Invoke(this, location);
            };

            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) =>
            {
                GllReceived?.Invoke(this, location);
                GnssDataReceived?.Invoke(this, location);
            };

            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                GsaReceived?.Invoke(this, activeSatellites);
                GnssDataReceived?.Invoke(this, activeSatellites);
            };

            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                RmcReceived?.Invoke(this, positionCourseAndTime);
                GnssDataReceived?.Invoke(this, positionCourseAndTime);
            };

            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                VtgReceived?.Invoke(this, courseAndVelocity);
                GnssDataReceived?.Invoke(this, courseAndVelocity);
            };

            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) =>
            {
                GsvReceived?.Invoke(this, satellites);
                GnssDataReceived?.Invoke(this, satellites);
            };
        }

        void MessageReceived(object sender, SerialMessageData e)
        {
            string msg = e.GetMessageString(Encoding.ASCII);

            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}