using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
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
    public class GsvParser : INmeaParser
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
        public string Prefix {
            get { return "$GPGSV"; }
        }

        /// <summary>
        ///     Get the friendly (human readable) name for the decoder.
        /// </summary>
        public string Name {
            get { return "Satellites in view"; }
        }

        /// <summary>
        ///     Process the message from the GPS.
        /// </summary>
        /// <param name="data">String array of the elements of the message.</param>
        public void Process(NmeaSentence sentence)
        {
            if (OnSatellitesInViewReceived != null) {
                var thisSentenceNumber = int.Parse(sentence.DataElements[1]);
                if (_currentSentence == 0) {
                    if (thisSentenceNumber == 1) {
                        _satelliteList = new Satellite[int.Parse(sentence.DataElements[2])];
                        _currentSentence = 1;
                        _totalSentences = int.Parse(sentence.DataElements[0]);
                        _nextSatelliteEntry = 0;
                    }
                }
                if (thisSentenceNumber == _currentSentence) {
                    _currentSentence++;

                    int elevation, azimuth, signalToNoiseRatio;

                    for (var currentSatellite = 0; currentSatellite < ((sentence.DataElements.Count - 3) / 4); currentSatellite++) {
                        var satelliteBase = (currentSatellite * 4) + 4;
                        _satelliteList[_nextSatelliteEntry].ID = sentence.DataElements[satelliteBase];
                        if (int.TryParse(sentence.DataElements[satelliteBase + 0], out elevation)) {
                            _satelliteList[_nextSatelliteEntry].Elevation = elevation;
                        }
                        if (int.TryParse(sentence.DataElements[satelliteBase + 1], out azimuth)) {
                            _satelliteList[_nextSatelliteEntry].Azimuth = azimuth;
                        }
                        if (int.TryParse(sentence.DataElements[satelliteBase + 2], out signalToNoiseRatio)) {
                            _satelliteList[_nextSatelliteEntry].SignalTolNoiseRatio = signalToNoiseRatio;
                        }
                        _nextSatelliteEntry++;
                    }
                    if (_nextSatelliteEntry == _satelliteList.Length) {
                        _currentSentence = 0;
                        _totalSentences = -1;
                        _satelliteList = null;
                        _nextSatelliteEntry = 0;
                        OnSatellitesInViewReceived(this, _satelliteList);
                    }
                } else {
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