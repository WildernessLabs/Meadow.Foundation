using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss
{
    /// <summary>
    /// GSA decoder class
    /// </summary>
    public class GsaDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event raised when valid GSA data is received
        /// </summary>
        public event EventHandler<ActiveSatellites> ActiveSatellitesReceived = default!;

        /// <summary>
        /// Prefix for the GSA decoder
        /// </summary>
        public string Prefix => "GSA";

        /// <summary>
        /// Friendly name for the GSA messages.
        /// </summary>
        public string Name => "GSA - DOP and number of active satellites.";

        /// <summary>
        /// Process the data from a GSA message
        /// </summary>
        /// <param name="sentence">String array of the message components for a GSA message.</param>
        public void Process(NmeaSentence sentence)
        {
            var satellites = new ActiveSatellites();

            satellites.TalkerID = sentence.TalkerID;

            satellites.SatelliteSelection = sentence.DataElements[0].ToLower() switch
            {
                "a" => ActiveSatelliteSelection.Automatic,
                "m" => ActiveSatelliteSelection.Manual,
                _ => ActiveSatelliteSelection.Unknown,
            };

            if (int.TryParse(sentence.DataElements[1], out var dimensionalFixType))
            {
                satellites.Dimensions = (DimensionalFixType)dimensionalFixType;
            }

            var satelliteCount = 0;
            for (var index = 2; index < 14; index++)
            {
                if (!string.IsNullOrEmpty(sentence.DataElements[index]))
                {
                    satelliteCount++;
                }
            }
            if (satelliteCount > 0)
            {
                satellites.SatellitesUsedForFix = new string[satelliteCount];
                var currentSatellite = 0;
                for (var index = 2; index < 14; index++)
                {
                    if (!string.IsNullOrEmpty(sentence.DataElements[index]))
                    {
                        satellites.SatellitesUsedForFix[currentSatellite] = sentence.DataElements[index];
                        currentSatellite++;
                    }
                }
            }
            else
            {
                satellites.SatellitesUsedForFix = null;
            }

            if (decimal.TryParse(sentence.DataElements[14], out var dilutionOfPrecision))
            {
                satellites.DilutionOfPrecision = dilutionOfPrecision;
            }

            if (decimal.TryParse(sentence.DataElements[15], out var horizontalDilutionOfPrecision))
            {
                satellites.HorizontalDilutionOfPrecision = horizontalDilutionOfPrecision;
            }

            if (decimal.TryParse(sentence.DataElements[16], out var verticalDilutionOfPrecision))
            {
                satellites.VerticalDilutionOfPrecision = verticalDilutionOfPrecision;
            }

            ActiveSatellitesReceived(this, satellites);
        }
    }
}