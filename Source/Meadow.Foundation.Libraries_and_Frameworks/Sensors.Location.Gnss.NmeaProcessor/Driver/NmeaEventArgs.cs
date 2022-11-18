namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// NMEA Event args - holds an NMEA sentence as a string
    /// </summary>
    public class NmeaEventArgs
    {
        /// <summary>
        /// The NMEA sentence
        /// </summary>
        public string NmeaSentence { get; set; } = string.Empty;
    }
}