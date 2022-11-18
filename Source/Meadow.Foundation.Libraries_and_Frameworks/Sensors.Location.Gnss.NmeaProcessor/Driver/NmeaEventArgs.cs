namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// NMEA Event args - holds an NMEA sentance as a string
    /// </summary>
    public class NmeaEventArgs
    {
        /// <summary>
        /// The NMEA sentance
        /// </summary>
        public string NmeaSentence { get; set; } = string.Empty;
    }
}