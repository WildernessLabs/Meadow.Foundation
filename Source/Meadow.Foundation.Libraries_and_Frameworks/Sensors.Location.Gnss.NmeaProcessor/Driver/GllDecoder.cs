using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// Process GLL (Geographic position Latitude / Longitude) messages from a
    /// GPS receiver.
    /// </summary>
    public class GllDecoder : INmeaDecoder
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
        public string Name => "Geographic Position - Latitude/Longitude";

        /// <summary>
        /// Process a GPGLL sentence string
        /// </summary>
        /// <param name="sentence">The raw NMEA sentence string</param>
        public void Process(string sentence)
        {
            if (!NmeaSentence.TryParse(sentence, out var s))
            {
                Resolver.Log.Debug($"Failure parsing {sentence}", Constants.LogGroup);
                return;
            }
            Process(s);
        }

        /// <summary>
        /// Process the data from a GLL message.
        /// </summary>
        /// <param name="sentence">String array of the message components for a GLL message.</param>
        public void Process(NmeaSentence sentence)
        {
            // Status is at index 5 (6th field), A = valid, V = not valid.
            var location = new GnssPositionInfo();

            location.IsValid = sentence.DataElements[5].ToLower() == "a";

            if (location.IsValid)
            {
                location.Position = new();
                location.TalkerID = sentence.TalkerID;
                location.Position.Latitude = NmeaUtilities.ParseLatitude(sentence.DataElements[0], sentence.DataElements[1]);
                location.Position.Longitude = NmeaUtilities.ParseLongitude(sentence.DataElements[2], sentence.DataElements[3]);
                location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[4]);
            }

            PositionReceived?.Invoke(this, location);
        }
    }
}