namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Process the Satellites in view messages from a GPS module.
    /// </summary>
    /// <remarks>
    ///     The satellites in view messages can contain multiple packets one for
    ///     each satelite.  There can also be multiple messages making up the total list
    ///     of satelites.
    ///     This class brings all of the messages together in a single message for the
    ///     consumer.
    /// </remarks>
    public class GSVDecoder : NMEADecoder
    {
        #region Member variables / fields

        /// <summary>
        ///     Current sentence being processed, 0 indicates nothing being processed.
        /// </summary>
        private int _currentSentence;

        /// <summary>
        ///     Total number of sentences expected in the sequence, -1 indicates nothing
        ///     is being processed at the moment.
        /// </summary>
        private int _totalSentences = -1;

        /// <summary>
        ///     List of satellites.
        /// </summary>
        private Satellite[] _satelliteList;

        /// <summary>
        ///     Next entry in the _satelliteList to receive satellite data.
        /// </summary>
        private int _nextSatelliteEntry;

        #endregion Member variables / fields

        #region Delegates and events

        /// <summary>
        ///     Delegate for the GSV data received event.
        /// </summary>
        /// <param name="activeSatellites">Active satellites.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void SatellitesInViewReceived(object sender, Satellite[] satellites);

        /// <summary>
        ///     Event raised when valid GSV data is received.
        /// </summary>
        public event SatellitesInViewReceived OnSatellitesInViewReceived;

        #endregion Delegates and events

        #region NMEADecoder methods & properties

        /// <summary>
        ///     Get the prefix for the decoder.
        /// </summary>
        /// <remarks>
        ///     The lines of text from the GPS start with text such as $GPGGA, $GPGLL, $GPGSA etc.  The prefix
        ///     is the start of the line (i.e. $GPCGA).
        /// </remarks>
        public override string Prefix
        {
            get { return"$GPGSV"; }
        }

        /// <summary>
        ///     Get the friendly (human readable) name for the decoder.
        /// </summary>
        public override string Name
        {
            get { return"Satellites in view"; }
        }

        /// <summary>
        ///     Process the message from the GPS.
        /// </summary>
        /// <param name="data">String array of the elements of the message.</param>
        public override void Process(string[] data)
        {
            if (OnSatellitesInViewReceived != null)
            {
                var thisSentenceNumber = int.Parse(data[2]);
                if (_currentSentence == 0)
                {
                    if (thisSentenceNumber == 1)
                    {
                        _satelliteList = new Satellite[int.Parse(data[3])];
                        _currentSentence = 1;
                        _totalSentences = int.Parse(data[1]);
                        _nextSatelliteEntry = 0;
                    }
                }
                if (thisSentenceNumber == _currentSentence)
                {
                    _currentSentence++;
                    for (var currentSatellite = 0; currentSatellite < ((data.Length - 4) / 4); currentSatellite++)
                    {
                        var satelliteBase = (currentSatellite * 4) + 4;
                        _satelliteList[_nextSatelliteEntry].ID = data[satelliteBase];
                        _satelliteList[_nextSatelliteEntry].Elevation = int.Parse(data[satelliteBase + 1]);
                        _satelliteList[_nextSatelliteEntry].Azimuth = int.Parse(data[satelliteBase + 2]);
                        _satelliteList[_nextSatelliteEntry].SignalTolNoiseRatio = int.Parse(data[satelliteBase + 3]);
                        _nextSatelliteEntry++;
                    }
                    if (_nextSatelliteEntry == _satelliteList.Length)
                    {
                        _currentSentence = 0;
                        _totalSentences = -1;
                        _satelliteList = null;
                        _nextSatelliteEntry = 0;
                        OnSatellitesInViewReceived(this, _satelliteList);
                    }
                }
                else
                {
                    //
                    //  If the application gets here then there is a problem with
                    //  the sequencing of the sentences.  Throw away the data received
                    //  so far and reset to pick up a new sequence of sentences.
                    //
                    _currentSentence = 0;
                    _totalSentences = -1;
                    _satelliteList = null;
                    _nextSatelliteEntry = 0;
                }
            }
        }

        #endregion NMEADecoder methods & properties
    }
}