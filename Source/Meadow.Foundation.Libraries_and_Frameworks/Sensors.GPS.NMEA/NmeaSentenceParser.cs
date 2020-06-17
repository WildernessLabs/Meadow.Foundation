using System;
using System.Collections;
using Meadow.Utilities;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.GPS
{

    /// <summary>
    /// NMEA GPS/GNSS sentence parser
    /// </summary>
    public class NmeaSentenceParser
    {
        /// <summary>
        /// NMEA decoders available to the GPS.
        /// </summary>
        private readonly Hashtable decoders = new Hashtable();

        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Default constructor for a NMEA GPS object, this is private to prevent the user from
        /// using it.
        /// </summary>
        public NmeaSentenceParser()
        {
        }

        /// <summary>
        ///     Add a new NMEA decoder to the GPS.
        /// </summary>
        /// <param name="decoder">NMEA decoder.</param>
        public void AddDecoder(INmeaParser decoder)
        {
            if (decoders.Contains(decoder.Prefix)) {
                throw new Exception(decoder.Prefix + " already registered.");
            }
            decoders.Add(decoder.Prefix, decoder);
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

            Console.WriteLine($"Sentence parsed: {sentence.ToString()}");

            var decoder = (INmeaParser)decoders[sentence.Prefix];
            if (decoder != null) {
                if (DebugMode) { Console.WriteLine($"Found appropriate decoder:{decoder.Prefix}"); }
                decoder.Process(sentence);
            } else {
                if (DebugMode) { Console.WriteLine($"Could not find appropriate decoder for {sentence.Prefix}"); }
            }
        }
    }
}