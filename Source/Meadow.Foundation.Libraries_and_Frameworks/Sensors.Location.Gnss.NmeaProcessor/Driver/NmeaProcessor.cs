using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// An engine that processes NMEA GPS/GNSS sentences by calling the appropriate
    /// decoder and handing them off. Note that it's designed to be asynchronous
    /// because certain messages (like $GPGSV/Satellites in View) need to be
    /// processed together in order to make sense.
    /// </summary>
    /// <remarks>
    /// To use, call `RegisterDecoder` for each NMEA sentence decoder that you
    /// want to use, passing an `INmeaDecoder`, and then call `ParseNmeaMessage`,
    /// and pass the NMEA sentence string,
    /// e.g. "$GPRMC,000049.799,V,,,,,0.00,0.00,060180,,,N*48".
    ///
    /// Each `INmeaDecoder` decoder has its own event(s) that can then be subscribed
    /// to, in order to get the resulting information.
    /// If you'd like to add additional decoders, an excellent reference on NMEA
    /// sentences can found [here](https://gpsd.gitlab.io/gpsd/NMEA.html).
    /// </remarks>
    public class NmeaSentenceProcessor
    {
        /// <summary>
        /// NMEA decoders available to the GPS
        /// </summary>
        private readonly Dictionary<string, INmeaDecoder/*<IGnssResult>*/> decoders = new Dictionary<string, INmeaDecoder/*<IGnssResult>*/>();

        // ToDo - remove this
        /// <summary>
        /// Enable / disable debug mode
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Creates a new instance of the NmeaSentenceProcessor
        /// </summary>
        public NmeaSentenceProcessor()
        { }

        /// <summary>
        /// Add a new NMEA decoder to the GPS
        /// </summary>
        /// <param name="decoder">NMEA decoder</param>
        public void RegisterDecoder(INmeaDecoder decoder)
        {
            Resolver.Log.Info($"Registering decoder: {decoder.Prefix}");
            if (decoders.ContainsKey(decoder.Prefix))
            {
                throw new Exception(decoder.Prefix + " already registered.");
            }
            decoders.Add(decoder.Prefix, decoder);
        }

        /// <summary>
        /// GPS message ready for processing
        /// </summary>
        /// <remarks>
        /// Unknown message types will be discarded
        /// </remarks>
        /// <param name="line">GPS text for processing</param>
        public void ProcessNmeaMessage(string line)
        {
            if (DebugMode) { Resolver.Log.Info("NmeaSentenceProcessor.ProcessNmeaMessage"); }

            NmeaSentence sentence;
            try
            {
                sentence = NmeaSentence.From(line);
            }
            catch (Exception e)
            {
                if (DebugMode) { Resolver.Log.Warn($"Could not parse message. {e.Message}"); }
                return;
            }

            INmeaDecoder decoder;
            if (decoders.ContainsKey(sentence.Prefix!))
            {
                decoder = decoders[sentence.Prefix!];
                if (decoder != null)
                {
                    if (DebugMode) { Resolver.Log.Info($"Found appropriate decoder:{decoder.Prefix}"); }
                    decoder.Process(sentence);
                }
            }
            else
            {
                if (DebugMode) { Resolver.Log.Warn($"Could not find appropriate decoder for {sentence.Prefix}"); }
            }
        }
    }
}