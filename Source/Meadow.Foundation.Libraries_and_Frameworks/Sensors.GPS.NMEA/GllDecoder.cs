using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Process GLL (Geographic position Latitude / Longitude) messages from a
    /// GPS receiver.
    /// </summary>
    public class GllDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event raised when valid GLL data is received.
        /// </summary>
        public event EventHandler<GnssPositionInfo> GeographicLatitudeLongitudeReceived = delegate { };

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix
        {
            get => "GLL";
        }

        //public bool DebugMode { get; set; } = true;

        /// <summary>
        /// Friendly name for the GLL messages.
        /// </summary>
        public string Name
        {
            get => "GLL - Global Postioning System Fix Data";
        }

        /// <summary>
        /// Process the data from a GLL message.
        /// </summary>
        /// <param name="data">String array of the message components for a GLL message.</param>
        public void Process(NmeaSentence sentence)
        {
            //
            //  Status is stored in element 7 (position 6), A = valid, V = not valid.
            //
            var location = new GnssPositionInfo();
            location.TalkerID = sentence.TalkerID;
            location.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[0], sentence.DataElements[1]);
            location.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[2], sentence.DataElements[3]);
            location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[4]);
            location.Valid = (sentence.DataElements[5].ToLower() == "a");
            GeographicLatitudeLongitudeReceived(this, location);
        }
    }
}