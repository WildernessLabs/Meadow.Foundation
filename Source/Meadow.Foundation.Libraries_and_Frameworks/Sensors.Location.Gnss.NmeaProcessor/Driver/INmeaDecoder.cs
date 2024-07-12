using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// Base class for NMEA sentence decoder classes
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
    /// Process the message from the GPS
    /// </summary>
    /// <param name="sentence">String array of the elements of the message</param>
    void Process(NmeaSentence sentence);
}