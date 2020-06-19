using System;
using System.Collections.Generic;
using Meadow.Utilities;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{

    /// <summary>
    /// An engine that processes NMEA GPS/GNSS sentences by calling the appropriate
    /// parser and handing them off. Note that it's designed to be asynchronous
    /// because certain messages (like $GPGSV/Satellites in View) need to be
    /// processed together in order to make sense.
    ///
    /// To use, call `RegisterParser` for each NMEA sentence parser that you
    /// want to use, passing an `INmeaParser`, and then call `ParseNmeaMessage`,
    /// and pass the NMEA sentence string,
    /// e.g. "$GPRMC,000049.799,V,,,,,0.00,0.00,060180,,,N*48".
    ///
    /// Each `INmeaParser` parser has 
    /// </summary>
    public class NmeaSentenceProcessor
    {
        /// <summary>
        /// NMEA parsers available to the GPS.
        /// </summary>
        private readonly Dictionary<string, INmeaParser/*<IGnssResult>*/> parsers = new Dictionary<string, INmeaParser/*<IGnssResult>*/>();

        public bool DebugMode { get; set; } = true;

        /// <summary>
        /// Default constructor for a NMEA GPS object, this is private to prevent the user from
        /// using it.
        /// </summary>
        public NmeaSentenceProcessor()
        {
        }

        /// <summary>
        /// Add a new NMEA parser to the GPS.
        /// </summary>
        /// <param name="parser">NMEA parser.</param>
        public void RegisterParser(INmeaParser/*<IGnssResult>*/ parser)
        {
            Console.WriteLine($"Registering parser: {parser.Prefix}");
            if (parsers.ContainsKey(parser.Prefix)) {
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

            INmeaParser parser;
            if (parsers.ContainsKey(sentence.Prefix)) {
                parser = parsers[sentence.Prefix];
                if (parser != null) {
                    if (DebugMode) { Console.WriteLine($"Found appropriate parser:{parser.Prefix}"); }
                    parser.Process(sentence);
                }
            } else {
                if (DebugMode) { Console.WriteLine($"Could not find appropriate parser for {sentence.Prefix}"); }
            }
        }
    }
}