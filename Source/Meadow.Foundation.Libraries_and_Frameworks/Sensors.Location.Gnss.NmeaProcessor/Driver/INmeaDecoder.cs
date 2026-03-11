using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// Interface for NMEA sentence decoder classes
/// </summary>
public interface INmeaDecoder
{
    /// <summary>
    /// Prefix for the decoder (text that occurs at the start of a GPS message
    /// including the $ symbol - $GPGSA etc.)
    /// </summary>
    string Prefix { get; }

    /// <summary>
    /// Friendly name for the decoder
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Process a raw NMEA sentence string
    /// </summary>
    /// <param name="sentence">The raw NMEA sentence string</param>
    void Process(string sentence);

    /// <summary>
    /// Process the message from the GPS
    /// </summary>
    /// <param name="sentence">Parsed NMEA sentence</param>
    void Process(NmeaSentence sentence);
}