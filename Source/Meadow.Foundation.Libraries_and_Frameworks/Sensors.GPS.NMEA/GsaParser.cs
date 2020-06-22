using System;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    public class GsaParser : INmeaParser
    {
        /// <summary>
        /// Event raised when valid GSA data is received.
        /// </summary>
        public event EventHandler<ActiveSatellites> ActiveSatellitesReceived = delegate { };

        /// <summary>
        /// Prefix for the GSA decoder.
        /// </summary>
        public string Prefix {
            get { return "GSA"; }
        }

        /// <summary>
        /// Friendly name for the GSA messages.
        /// </summary>
        public string Name {
            get { return "GSA - DOP and number of active satellites."; }
        }

        /// <summary>
        /// Process the data from a GSA message.
        /// </summary>
        /// <param name="data">String array of the message components for a GSA message.</param>
        public void Process(NmeaSentence sentence)
        {
            //Console.WriteLine($"GSADecoder.Process");

            var satellites = new ActiveSatellites();

            satellites.TalkerID = sentence.TalkerID;

            switch (sentence.DataElements[0].ToLower()) {
                case "a":
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Automatic;
                    break;
                case "m":
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Manual;
                    break;
                default:
                    satellites.SatelliteSelection = ActiveSatelliteSelection.Unknown;
                    break;
            }

            int dimensionalFixType;
            if (int.TryParse(sentence.DataElements[1], out dimensionalFixType)) {
                satellites.Dimensions = (DimensionalFixType)dimensionalFixType;
            }

            var satelliteCount = 0;
            for (var index = 2; index < 14; index++) {
                if (!string.IsNullOrEmpty(sentence.DataElements[index])) {
                    satelliteCount++;
                }
            }
            if (satelliteCount > 0) {
                satellites.SatellitesUsedForFix = new string[satelliteCount];
                var currentSatellite = 0;
                for (var index = 2; index < 14; index++) {
                    if (!string.IsNullOrEmpty(sentence.DataElements[index])) {
                        satellites.SatellitesUsedForFix[currentSatellite] = sentence.DataElements[index];
                        currentSatellite++;
                    }
                }
            } else {
                satellites.SatellitesUsedForFix = null;
            }
            decimal dilutionOfPrecision;
            if (decimal.TryParse(sentence.DataElements[14], out dilutionOfPrecision)) {
                satellites.DilutionOfPrecision = dilutionOfPrecision;
            }
            decimal horizontalDilutionOfPrecision;
            if (decimal.TryParse(sentence.DataElements[15], out horizontalDilutionOfPrecision)) {
                satellites.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }
            decimal verticalDilutionOfPrecision;
            if (decimal.TryParse(sentence.DataElements[16], out verticalDilutionOfPrecision)) {
                satellites.VerticalDilutionOfPrecision = verticalDilutionOfPrecision;
            }
            //Console.WriteLine($"GSADecoder.Process complete; satelliteCount:{satelliteCount}");
            ActiveSatellitesReceived(this, satellites);
        }
    }
}