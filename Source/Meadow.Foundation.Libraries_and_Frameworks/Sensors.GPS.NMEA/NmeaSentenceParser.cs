using System;
using System.Collections;
using Meadow.Utilities;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{

    /// <summary>
    /// NMEA GPS/GNSS sentence parser
    /// </summary>
    public class NmeaSentenceParser
    {
        /// <summary>
        /// NMEA parsers available to the GPS.
        /// </summary>
        private readonly Hashtable parsers = new Hashtable();

        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Default constructor for a NMEA GPS object, this is private to prevent the user from
        /// using it.
        /// </summary>
        public NmeaSentenceParser()
        {
        }

        /// <summary>
        /// Add a new NMEA parser to the GPS.
        /// </summary>
        /// <param name="parser">NMEA parser.</param>
        public void AddParser(INmeaParser parser)
        {
            if (parsers.Contains(parser.Prefix)) {
                throw new Exception(parser.Prefix + " already registered.");
            }
            parsers.Add(parser.Prefix, parser);
        }

        /// <summary>
        ///     GPS message ready for processing.
        /// </summary>
        /// <remarks>
        ///     Unknown message types will be discarded.
        /// </remarks>
        /// <param name="line">GPS text for processing.</param>
        public void ParseNmeaMessage(string line)
        {
            if (DebugMode) { Console.WriteLine("NmeaSentenceParser.ParseNmeaMessage"); }

            // create a NmeaSentence from the sentence string
            NmeaSentence sentence;
            try {
                sentence = NmeaSentence.From(line);
            } catch (Exception e) {
                if (DebugMode) { Console.WriteLine($"Could not parse message. {e.Message}"); }
                return;
            }

            //Console.WriteLine($"Sentence parsed: {sentence.ToString()}");

            var parser = (INmeaParser)parsers[sentence.Prefix];
            if (parser != null) {
                if (DebugMode) { Console.WriteLine($"Found appropriate parser:{parser.Prefix}"); }
                parser.Process(sentence);
            } else {
                if (DebugMode) { Console.WriteLine($"Could not find appropriate parser for {sentence.Prefix}"); }
            }
        }
    }
}