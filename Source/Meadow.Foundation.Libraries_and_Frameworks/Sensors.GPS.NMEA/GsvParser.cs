using System;
using System.Collections.Generic;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Process the Satellites in view messages from a GPS module.
    /// </summary>
    /// <remarks>
    /// The satellites in view messages can contain multiple sentences; one for
    /// each satelite. There can also be multiple messages making up the total list
    /// of satelites.
    /// This class brings all of the messages together in a single message for the
    /// consumer.
    /// </remarks>
    public class GsvParser : INmeaParser
    {
        /// <summary>
        /// Event raised when valid GSV data is received.
        /// </summary>
        public event EventHandler<List<Satellite>> SatellitesInViewReceived = delegate { };

        /// <summary>
        /// Current sentence being processed, 0 indicates nothing being processed.
        /// </summary>
        private int _currentSentence;

        /// <summary>
        /// Total number of sentences expected in the sequence, -1 indicates nothing
        /// is being processed at the moment.
        /// </summary>
        private int _totalSentences = -1;

        /// <summary>
        /// List of satellites.
        /// </summary>
        protected List<Satellite> _satelliteList;// = new Satellites();

        /// <summary>
        /// Next entry in the _satelliteList to receive satellite data.
        /// </summary>
        private int _nextSatelliteEntry;

        /// <summary>
        /// Get the prefix for the decoder.
        /// </summary>
        /// <remarks>
        /// The lines of text from the GPS start with text such as $GPGGA, $GPGLL, $GPGSA etc.  The prefix
        /// is the start of the line (i.e. $GPCGA).
        /// </remarks>
        public string Prefix {
            get { return "$GPGSV"; }
        }

        /// <summary>
        /// Get the friendly (human readable) name for the decoder.
        /// </summary>
        public string Name {
            get { return "Satellites in view"; }
        }

        /// <summary>
        /// Process the message from the GPS.
        /// </summary>
        /// <param name="data">String array of the elements of the message.</param>
        /// https://gpsd.gitlab.io/gpsd/NMEA.html#_gsv_satellites_in_view
        public void Process(NmeaSentence sentence)
        {
            // Sentence number, 1-9 of this GSV message within current sentence group
            var thisSentenceNumber = int.Parse(sentence.DataElements[1]);

            // if it's the first time we've gotten a sentence in this sequence
            if (_currentSentence == 0) {
                // if it's the first sentence, it'll have the total number of sentences
                // in the group
                if (thisSentenceNumber == 1) {
                    // create our collection
                    this._satelliteList = new List<Satellite>();

                    // total number of satellites
                    //_satelliteList = new Satellite[int.Parse(sentence.DataElements[2])];
                    // iterate our sentence count so we don't parse this out again
                    _currentSentence = 1;
                    // how many sentences to expect in this group
                    _totalSentences = int.Parse(sentence.DataElements[0]);
                    _nextSatelliteEntry = 0;
                }
            }
            // make sure we're getting these in order (BryanC note: i'm not sure this is necessary)
            if (thisSentenceNumber == _currentSentence) {
                _currentSentence++;

                int id, elevation, azimuth, signalToNoiseRatio;
                Satellite sat;

                // for each satellite within the sentence
                int currentSatIndex = 0;
                int numberOfSatsInSentence = (sentence.DataElements.Count - 3) / 4;
                //Console.WriteLine($"# of satellites in this sentence: {numberOfSatsInSentence}");
                for ( var currentSatellite = 0; currentSatellite < numberOfSatsInSentence; currentSatellite++) {
                    //Console.WriteLine($"Satellite: {currentSatellite}");
                    // calculate index (for each satellite)
                    currentSatIndex = (currentSatellite * 4) + 3;

                    //Console.WriteLine($"Satellite index in sentence: {currentSatIndex}");

                    sat = new Satellite();

                    if (int.TryParse(sentence.DataElements[currentSatIndex + 0], out id)) {
                        sat.ID = id;
                        //Console.WriteLine($"Sat ID: {sat.ID}");
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 1], out elevation)) {
                        sat.Elevation = elevation;
                        //Console.WriteLine($"Sat Elevation: {sat.Elevation}");
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 2], out azimuth)) {
                        sat.Azimuth = azimuth;
                        //Console.WriteLine($"Sat Azimuth: {sat.Azimuth}");
                    }
                    if (int.TryParse(sentence.DataElements[currentSatIndex + 3], out signalToNoiseRatio)) {
                        sat.SignalTolNoiseRatio = signalToNoiseRatio;
                        //Console.WriteLine($"Sat SignalTolNoiseRatio: {sat.SignalTolNoiseRatio}");
                    }
                    //Console.WriteLine($"Adding satellite: {currentSatellite}");
                    this._satelliteList.Add(sat);

                    _nextSatelliteEntry++;
                }
                // if we got them all
                if (_nextSatelliteEntry == _satelliteList.Count) {
                    //Console.WriteLine("Done with satellites in this sentence.");
                    _currentSentence = 0;
                    _totalSentences = -1;
                    //_satelliteList = null;
                    _nextSatelliteEntry = 0;
                    SatellitesInViewReceived(this, _satelliteList);
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
}