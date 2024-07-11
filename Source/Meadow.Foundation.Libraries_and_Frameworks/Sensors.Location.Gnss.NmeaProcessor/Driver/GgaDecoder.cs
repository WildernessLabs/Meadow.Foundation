using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    // TODO: Should this be a struct with fields?
    /// <summary>
    /// Decoder for GGA messages.
    /// </summary>
    public class GgaDecoder : INmeaDecoder, IGnssPositionEventSource
    {
        /// <inheritdoc/>
        public event EventHandler<GnssPositionInfo>? PositionReceived;

        /// <summary>
        /// Prefix for the GGA decoder.
        /// </summary>
        public string Prefix => "GGA";

        /// <summary>
        /// Friendly name for the GGA messages.
        /// </summary>
        public string Name => "Global Positioning System Fix Data";

        /// <summary>
        /// Process a GPGGA sentence string
        /// </summary>
        /// <param name="sentence"></param>
        public void Process(string sentence)
        {
            Process(NmeaSentence.From(sentence));
        }

        /// <summary>
        /// Process the data from a GGA message
        /// </summary>
        /// <param name="sentence">String array of the message components for a CGA message</param>
        public void Process(NmeaSentence sentence)
        {
            // make sure all fields are present
            for (var index = 0; index <= 7; index++)
            {
                if (string.IsNullOrEmpty(sentence.DataElements[index]))
                {
                    //Resolver.Log.Warn("Not all elements present");
                    // TODO: should we throw an exception and have callers wrap in a try/catch?
                    // problem today is that it just quietly returns
                    return;
                }
            }

            var location = new GnssPositionInfo();
            location.Position = new();
            location.TalkerID = sentence.TalkerID;
            location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[0]);
            location.Position!.Latitude = NmeaUtilities.ParseLatitude(sentence.DataElements[1], sentence.DataElements[2]);
            location.Position.Longitude = NmeaUtilities.ParseLongitude(sentence.DataElements[3], sentence.DataElements[4]);
            location.FixQuality = (FixType)int.Parse(sentence.DataElements[5]);

            if (int.TryParse(sentence.DataElements[6], out var numberOfSatellites))
            {
                location.NumberOfSatellites = numberOfSatellites;
            }

            if (decimal.TryParse(sentence.DataElements[7], out var horizontalDilutionOfPrecision))
            {
                location.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }

            if (double.TryParse(sentence.DataElements[8], out var altitude))
            {
                location.Position.Altitude = new Units.Length(altitude, Units.Length.UnitType.Meters);
            }

            PositionReceived?.Invoke(this, location);
        }
    }
}
