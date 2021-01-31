using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    // TODO: Should this be a struct with fields?
    /// <summary>
    /// Decoder for GGA messages.
    /// </summary>
    public class GgaDecoder : INmeaDecoder
    {
        /// <summary>
        /// Position update received event.
        /// </summary>
        public event EventHandler<GnssPositionInfo> PositionReceived = delegate { };

        /// <summary>
        /// Prefix for the GGA decoder.
        /// </summary>
        public string Prefix {
            get { return "GGA"; }
        }

        /// <summary>
        /// Friendly name for the GGA messages.
        /// </summary>
        public string Name {
            get { return "Global Postioning System Fix Data"; }
        }

        /// <summary>
        /// Process the data from a GGA message.
        /// </summary>
        /// <param name="data">String array of the message components for a CGA message.</param>
        public void Process(NmeaSentence sentence)
        {
            // make sure all fields are present
            for (var index = 0; index <= 7; index++) {
                if (string.IsNullOrEmpty(sentence.DataElements[index])) {
                    //Console.WriteLine("Not all elements present");
                    // TODO: should we throw an exception and have callers wrap in a try/catch?
                    // problem today is that it just quietly returns
                    return;
                }
            }

            var location = new GnssPositionInfo();
            location.TalkerID = sentence.TalkerID;
            location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[0]);
            location.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[1], sentence.DataElements[2]);
            location.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[3], sentence.DataElements[4]);
            location.FixQuality = (FixType)Converters.Integer(sentence.DataElements[5]);

            int numberOfSatellites;
            if (int.TryParse(sentence.DataElements[6], out numberOfSatellites)) {
                location.NumberOfSatellites = numberOfSatellites;
            }
            decimal horizontalDilutionOfPrecision;
            if (decimal.TryParse(sentence.DataElements[7], out horizontalDilutionOfPrecision)) {
                location.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }
            decimal altitude;
            if (decimal.TryParse(sentence.DataElements[8], out altitude)) {
                location.Position.Altitude = altitude;
            }
            PositionReceived(this, location);
        }
    }
}