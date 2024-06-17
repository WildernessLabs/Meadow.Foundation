using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// Process GLL (Geographic position Latitude / Longitude) messages from a
    /// GPS receiver.
    /// </summary>
    public class GllDecoder : INmeaDecoder, IGnssPositionEventSource
    {
        /// <inheritdoc/>
        public event EventHandler<GnssPositionInfo>? PositionReceived;

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix => "GLL";

        /// <summary>
        /// Friendly name for the GLL messages.
        /// </summary>
        public string Name => "GLL - Global Positioning System Fix Data";

        /// <summary>
        /// Process a GPRMC sentence string
        /// </summary>
        /// <param name="sentence">The sentence</param>
        public void Process(string sentence)
        {
            Process(NmeaSentence.From(sentence));
        }

        /// <summary>
        /// Process the data from a GLL message.
        /// </summary>
        /// <param name="sentence">String array of the message components for a GLL message.</param>
        public void Process(NmeaSentence sentence)
        {
            //
            //  Status is stored in element 7 (position 6), A = valid, V = not valid.
            //
            var location = new GnssPositionInfo();
            location.Position = new();
            location.TalkerID = sentence.TalkerID;
            location.Position.Latitude = NmeaUtilities.ParseLatitude(sentence.DataElements[0], sentence.DataElements[1]);
            location.Position.Longitude = NmeaUtilities.ParseLongitude(sentence.DataElements[2], sentence.DataElements[3]);
            location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[4]);
            location.IsValid = (sentence.DataElements[5].ToLower() == "a");
            PositionReceived?.Invoke(this, location);
        }
    }
}