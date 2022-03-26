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
    public class GsvDecoder : INmeaDecoder
    {
        // Note this is commented out so we don't pay the (trivial) price of the if() check. :)
        //protected bool DebugMode { get; set; } = false;

        /// <summary>
        /// Event raised when valid GSV data is received.
        /// </summary>
        //public event EventHandler<Satellite[]> SatellitesInViewReceived = delegate { };
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

        //private int _totalNumberOfSatellites = 0;

        /// <summary>
        /// List of satellites.
        /// </summary>
        //protected List<Satellite> _satelliteList;// = new Satellites();
        protected Satellite[] _satellites;

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
        public string Prefix {
            get => "GSV";
        }

        /// <summary>
        /// Get the friendly (human readable) name for the decoder.
        /// </summary>
        public string Name {
            get => "Satellites in view";
        }

        /// <summary>
        /// Process the message from the GPS.
        /// </summary>
        /// <param name="data">String array of the elements of the message.</param>
        /// https://gpsd.gitlab.io/gpsd/NMEA.html#_gsv_satellites_in_view
        public void Process(NmeaSentence sentence)
        {
            //if (DebugMode) { Console.WriteLine($"GSV Parser, currentSentence:{_currentSentence}, totalSentences:{_totalSentences}"); }

            // Sentence number, 1-9 of this GSV message within current sentence group
            int thisSentenceNumber;
            if (!int.TryParse(sentence.DataElements[1], out thisSentenceNumber)) {
                //if (DebugMode) { Console.WriteLine("Could not parse sentence number"); }
                return;
            }

            //if (DebugMode) { Console.WriteLine($"thisSentenceNumber:{thisSentenceNumber}"); }

            // if it's the first time we've gotten a sentence in this sequence
            if (_currentSentence == 0) {
                // if it's the first sentence, it'll have the total number of sentences
                // in the group
                if (thisSentenceNumber == 1) { // 1-based index. 
                    // create our collection
                    //this._satelliteList = new List<Satellite>();

                    // total number of satellites
                    int totalNumberOfSatellites;
                    if (!int.TryParse(sentence.DataElements[2], out totalNumberOfSatellites)) {
                        //
                        //if (DebugMode) { Console.WriteLine("Could not parse total number of satellites, bailing out."); }
                        CleanUp();
                        return;
                    }
                    // new up our satellite array
                    _satellites = new Satellite[totalNumberOfSatellites];

                    // iterate our sentence count so we don't parse this out again
                    _currentSentence = 1;
                    // how many sentences to expect in this group
                    _totalSentences = int.Parse(sentence.DataElements[0]);

                    // start our index
                    _currentSatelliteIndex = 0;

                    //if (DebugMode) { Console.WriteLine($"Total number of sentences: {_totalSentences}"); }
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
                //if (DebugMode) { Console.WriteLine($"# of satellites in this sentence: {numberOfSatsInSentence}"); }
                for ( var currentSatellite = 0; currentSatellite < numberOfSatsInSentence; currentSatellite++) {
                    //if (DebugMode) { Console.WriteLine($"About to parse out satellite: {currentSatellite}"); }
                    // calculate index (for each satellite)
                    currentSatIndex = (currentSatellite * 4) + 3;

                    //if (DebugMode) { Console.WriteLine($"Satellite index in sentence: {currentSatIndex}"); }

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

                    //if (DebugMode) { Console.WriteLine($"Adding satellite: {currentSatellite}, total satellite index: {_currentSatelliteIndex}"); }
                    //this._satelliteList.Add(sat);

                    _satellites[_currentSatelliteIndex] = sat;

                    _currentSatelliteIndex++;

                    // if we got them all, we need to break out of the loop, because
                    // even though a sentence might contain room for 4 satellites,
                    // they might be blank.
                    if (_currentSatelliteIndex == _satellites.Length) {
                        break;
                    }
                }

                // if we got them all
                if (_currentSatelliteIndex == _satellites.Length) {

                    //if (DebugMode) { Console.WriteLine("Done with satellites in this sentence."); }
                    SatellitesInViewReceived(this, new SatellitesInView(_satellites) { TalkerID = sentence.TalkerID });
                    CleanUp();
                }
            } else {
                //if (DebugMode) { Console.WriteLine($"Why are we here?"); }
                //
                //  If the application gets here then there is a problem with
                //  the sequencing of the sentences.  Throw away the data received
                //  so far and reset to pick up a new sequence of sentences.
                //
                CleanUp();
            }
        }

        protected void CleanUp()
        {
            _currentSentence = 0;
            _totalSentences = -1;
            _satellites = null;
            _currentSatelliteIndex = 0;
        }
    }
}