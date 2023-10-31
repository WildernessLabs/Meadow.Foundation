using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// Process the Satellites in view messages from a GPS module.
    /// </summary>
    /// <remarks>
    /// The satellites in view messages can contain multiple sentences; one for
    /// each satellite. There can also be multiple messages making up the total list
    /// of satellites.
    /// This class brings all of the messages together in a single message for the
    /// consumer.
    /// </remarks>
    public class GsvDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event raised when valid GSV data is received.
        /// </summary>
        public event EventHandler<SatellitesInView> SatellitesInViewReceived = delegate { };

        /// <summary>
        /// Current sentence being processed, 0 indicates nothing being processed.
        /// </summary>
        private int _currentSentence = 0;

        /// <summary>
        /// Total number of sentences expected in the sequence, -1 indicates nothing
        /// is being processed at the moment.
        /// </summary>
        private int _totalSentences = -1;

        /// <summary>
        /// List of satellites.
        /// </summary>
        protected Satellite[]? _satellites;

        /// <summary>
        /// Current index of the satellite we're processing
        /// </summary>
        private int _currentSatelliteIndex = 0;

        /// <summary>
        /// Get the prefix for the decoder.
        /// </summary>
        /// <remarks>
        /// The lines of text from the GPS start with text such as $GPGGA, $GPGLL, $GPGSA etc.  The prefix
        /// is the start of the line (i.e. $GPCGA).
        /// </remarks>
        public string Prefix => "GSV";


        /// <summary>
        /// Get the friendly (human readable) name for the decoder.
        /// </summary>
        public string Name => "Satellites in view";

        /// <summary>
        /// Process the message from the GPS.
        /// </summary>
        /// <param name="sentence">String array of the elements of the message.</param>
        /// https://gpsd.gitlab.io/gpsd/NMEA.html#_gsv_satellites_in_view
        public void Process(NmeaSentence sentence)
        {
            // Sentence number, 1-9 of this GSV message within current sentence group
            if (!int.TryParse(sentence.DataElements[1], out int thisSentenceNumber))
            {
                return;
            }

            if (_currentSentence == 0)
            {
                if (thisSentenceNumber == 1)
                {
                    if (!int.TryParse(sentence.DataElements[2], out int totalNumberOfSatellites))
                    {
                        CleanUp();
                        return;
                    }
                    _satellites = new Satellite[totalNumberOfSatellites];

                    _currentSentence = 1;
                    _totalSentences = int.Parse(sentence.DataElements[0]);

                    _currentSatelliteIndex = 0;
                }
            }
            if (thisSentenceNumber == _currentSentence)
            {
                _currentSentence++;

                Satellite sat;

                int currentSatIndex = 0;
                int numberOfSatsInSentence = (sentence.DataElements.Count - 3) / 4;

                for (var currentSatellite = 0; currentSatellite < numberOfSatsInSentence; currentSatellite++)
                {

                    currentSatIndex = (currentSatellite * 4) + 3;

                    sat = new Satellite();

                    if (int.TryParse(sentence.DataElements[currentSatIndex + 0], out int id))
                    {
                        sat.ID = id;
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 1], out int elevation))
                    {
                        sat.Elevation = elevation;
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 2], out int azimuth))
                    {
                        sat.Azimuth = azimuth;
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 3], out int signalToNoiseRatio))
                    {
                        sat.SignalTolNoiseRatio = signalToNoiseRatio;
                    }

                    _satellites![_currentSatelliteIndex] = sat;

                    _currentSatelliteIndex++;

                    if (_currentSatelliteIndex == _satellites.Length)
                    {
                        break;
                    }
                }

                if (_currentSatelliteIndex == _satellites!.Length)
                {

                    SatellitesInViewReceived(this, new SatellitesInView(_satellites) { TalkerID = sentence.TalkerID });
                    CleanUp();
                }
            }
            else
            {
                CleanUp();
            }
        }

        void CleanUp()
        {
            _currentSentence = 0;
            _totalSentences = -1;
            _satellites = null;
            _currentSatelliteIndex = 0;
        }
    }
}