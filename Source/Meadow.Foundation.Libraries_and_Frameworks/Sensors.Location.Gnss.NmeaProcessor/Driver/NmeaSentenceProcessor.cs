using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// An engine that processes NMEA GPS/GNSS sentences by calling the appropriate
/// decoder and handing them off. Note that it's designed to be asynchronous
/// because certain messages (like $GPGSV/Satellites in View) need to be
/// processed together in order to make sense.
/// </summary>
/// <remarks>
/// To use, call `RegisterDecoder` for each NMEA sentence decoder that you
/// want to use, passing an `INmeaDecoder`, and then call `ProcessNmeaMessage`,
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
    private readonly Dictionary<string, INmeaDecoder> decoders = new Dictionary<string, INmeaDecoder>();

    /// <summary>
    /// Creates a new instance of the NmeaSentenceProcessor
    /// </summary>
    public NmeaSentenceProcessor()
    { }

    /// <summary>
    /// Registers an NMEA decoder for its sentence prefix
    /// </summary>
    /// <param name="decoder">NMEA decoder</param>
    /// <exception cref="Exception">Thrown if a decoder for the same prefix is already registered</exception>
    public void RegisterDecoder(INmeaDecoder decoder)
    {
        Resolver.Log.Trace($"Registering decoder: {decoder.Prefix}");
        if (decoders.ContainsKey(decoder.Prefix))
        {
            throw new Exception($"{decoder.Prefix} already registered.");
        }
        decoders.Add(decoder.Prefix, decoder);
    }

    /// <summary>
    /// Unregisters a previously registered NMEA decoder
    /// </summary>
    /// <param name="decoder">NMEA decoder to remove</param>
    public void UnregisterDecoder(INmeaDecoder decoder)
    {
        Resolver.Log.Trace($"Unregistering decoder: {decoder.Prefix}");
        decoders.Remove(decoder.Prefix);
    }

    /// <summary>
    /// Processes a raw NMEA sentence string, routing it to the appropriate registered decoder
    /// </summary>
    /// <remarks>
    /// Unknown message types will be discarded
    /// </remarks>
    /// <param name="line">Raw NMEA sentence string</param>
    public void ProcessNmeaMessage(string line)
    {
        Resolver.Log.Trace("NmeaSentenceProcessor.ProcessNmeaMessage");

        NmeaSentence sentence;
        try
        {
            sentence = NmeaSentence.From(line);
        }
        catch (Exception e)
        {
            Resolver.Log.Debug($"Could not parse message. {e.Message}");
            return;
        }

        if (decoders.TryGetValue(sentence.Prefix!, out var decoder))
        {
            Resolver.Log.Trace($"Found decoder for: {decoder.Prefix}");

            try
            {
                decoder.Process(sentence);
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"{ex.Message}{Environment.NewLine}Failed to process {sentence}");
            }
        }
        else
        {
            Resolver.Log.Trace($"No decoder registered for {sentence.Prefix}");
        }
    }
}