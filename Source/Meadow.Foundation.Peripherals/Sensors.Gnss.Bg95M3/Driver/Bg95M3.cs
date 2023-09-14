using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Gnss.Bg95M3
{
    /// <summary>
    /// Represents a BG95-M3 Cellular/GNSS module
    /// </summary>
    public class Bg95M3 : IGnssSensor
    {
        private bool isGnssUpdateEnabled;

        private TimeSpan _updatePeriod = TimeSpan.FromMinutes(30);

        private ICellNetworkAdapter _cellAdapter;

        private CancellationTokenSource cancellationTokenSource;

        private NmeaSentenceProcessor nmeaProcessor;
        
        private IGnssResult[] _supportedResultTypes = new IGnssResult[]
        {
            new GnssPositionInfo(),
            new ActiveSatellites(),
            new CourseOverGround(),
            new SatellitesInView(new Satellite[0]),
        };

        /// <summary>
        /// Initializes NMEA sentence decoders for GNSS data processing based on supported result types
        /// </summary>
        private void InitDecoders()
        {
            nmeaProcessor = new NmeaSentenceProcessor();

            foreach (var resultType in SupportedResultTypes)
            {
                if (resultType is GnssPositionInfo)
                {
                    var ggaDecoder = new GgaDecoder();
                    nmeaProcessor.RegisterDecoder(ggaDecoder);
                    ggaDecoder.PositionReceived += (sender, location) =>
                    {
                        GnssDataReceived(this, location);
                    };

                    var gllDecoder = new GllDecoder();
                    nmeaProcessor.RegisterDecoder(gllDecoder);
                    gllDecoder.GeographicLatitudeLongitudeReceived += (sender, location) =>
                    {
                        GnssDataReceived(this, location);
                    };

                    var rmcDecoder = new RmcDecoder();
                    nmeaProcessor.RegisterDecoder(rmcDecoder);
                    rmcDecoder.PositionCourseAndTimeReceived += (sender, positionCourseAndTime) =>
                    {
                        GnssDataReceived(this, positionCourseAndTime);
                    };
                }
                else if (resultType is ActiveSatellites)
                {
                    var gsaDecoder = new GsaDecoder();
                    nmeaProcessor.RegisterDecoder(gsaDecoder);
                    gsaDecoder.ActiveSatellitesReceived += (sender, activeSatellites) =>
                    {
                        GnssDataReceived(this, activeSatellites);
                    };
                }
                else if (resultType is CourseOverGround)
                {
                    var vtgDecoder = new VtgDecoder();
                    nmeaProcessor.RegisterDecoder(vtgDecoder);
                    vtgDecoder.CourseAndVelocityReceived += (sender, courseAndVelocity) =>
                    {
                        GnssDataReceived(this, courseAndVelocity);
                    };
                }
                else if (resultType is SatellitesInView)
                {
                    var gsvDecoder = new GsvDecoder();
                    nmeaProcessor.RegisterDecoder(gsvDecoder);
                    gsvDecoder.SatellitesInViewReceived += (sender, satellites) =>
                    {
                        GnssDataReceived(this, satellites);
                    };
                }
            }
        }

        /// <summary>
        /// Initializes the BG95-M3 module with the provided cell network adapter, update period and result types array
        /// </summary>
        /// <param name="cellAdapter">The cellular network adapter used for communication.</param>
        /// <param name="updatePeriod">The time interval between GNSS updates from the BG95-M3 module.</param>
        /// <param name="resultTypes">An array of supported GNSS result types for data processing.</param>
        private void Initialize(ICellNetworkAdapter cellAdapter, TimeSpan updatePeriod, IGnssResult[] resultTypes)
        {
            _cellAdapter = cellAdapter;
            _updatePeriod = updatePeriod;
            _supportedResultTypes = resultTypes;
            InitDecoders();
        }

        /// <summary>
        /// Supported GNSS result types
        /// </summary>
        public IGnssResult[] SupportedResultTypes => _supportedResultTypes;

        /// <summary>
        /// Raised when new GNSS data is available
        /// </summary>
        public event EventHandler<IGnssResult> GnssDataReceived = delegate { };

        /// <summary>
        /// Create a new BG95-M3 object with the default update period and with all the supported result types
        /// </summary>
        public Bg95M3(ICellNetworkAdapter cellAdapter)
        {
            Initialize(cellAdapter, _updatePeriod, _supportedResultTypes);
        }

        /// <summary>
        /// Create a new BG95-M3 object with all the supported result types
        /// </summary>
        public Bg95M3(ICellNetworkAdapter cellAdapter, TimeSpan updatePeriod)
        {
            Initialize(cellAdapter, updatePeriod, _supportedResultTypes);
        }

        /// <summary>
        /// Create a new BG95-M3 object
        /// </summary>
        public Bg95M3(ICellNetworkAdapter cellAdapter, TimeSpan updatePeriod, IGnssResult[] resultTypes)
        {
            Initialize(cellAdapter, updatePeriod, resultTypes);
        }

        /// <summary>
        /// Start updating
        /// </summary>
        public void StartUpdating()
        {
            isGnssUpdateEnabled = true;
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var gnssAtCmdsOutput = _cellAdapter.FetchGnssAtCmdsOutput(_supportedResultTypes);

                    string[] sentences = gnssAtCmdsOutput.Split(new char[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sentence in sentences)
                    {
                        nmeaProcessor.ProcessNmeaMessage(sentence);
                    }

                    await Task.Delay(_updatePeriod);
                }
            });
        }

        /// <summary>
        /// Stop updating GNSS data
        /// </summary>
        public void StopUpdating()
        {
            isGnssUpdateEnabled = false;

            // Request cancellation to stop the updating process gracefully.
            cancellationTokenSource?.Cancel();
            return;
        }
    }
}