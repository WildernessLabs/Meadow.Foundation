namespace Meadow.Foundation.Sensors.GPS
{
    public class GSADecoder : NMEADecoder
    {
        #region Delegates and events

        /// <summary>
        ///     Delegate for the GSA data received event.
        /// </summary>
        /// <param name="activeSatellites">Active satellites.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void ActiveSatellitesReceived(object sender, ActiveSatellites activeSatellites);

        /// <summary>
        ///     Event raised when valid GSA data is received.
        /// </summary>
        public event ActiveSatellitesReceived OnActiveSatellitesReceived;

        #endregion Delegates and events

        #region INMEADecoder methods & properties

        /// <summary>
        ///     Prefix for the GSA decoder.
        /// </summary>
        public override string Prefix
        {
            get { return "$GPGSA"; }
        }

        /// <summary>
        ///     Friendly name for the GSA messages.
        /// </summary>
        public override string Name
        {
            get { return "GSA - DOP and number of active satellites."; }
        }

        /// <summary>
        ///     Process the data from a GSA message.
        /// </summary>
        /// <param name="data">String array of the message components for a GSA message.</param>
        public override void Process(string[] data)
        {
            if (OnActiveSatellitesReceived != null)
            {
                var satellites = new ActiveSatellites();
                switch (data[1].ToLower())
                {
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
                satellites.Demensions = (DimensionalFixType) int.Parse(data[2]);
                var satelliteCount = 0;
                for (var index = 3; index < 15; index++)
                {
                    if ((data[index] != null) && (data[index] != ""))
                    {
                        satelliteCount++;
                    }
                }
                if (satelliteCount > 0)
                {
                    satellites.SatellitesUsedForFix = new string[satelliteCount];
                    var currentSatellite = 0;
                    for (var index = 3; index < 15; index++)
                    {
                        if ((data[index] != null) && (data[index] != ""))
                        {
                            satellites.SatellitesUsedForFix[currentSatellite] = data[index];
                            currentSatellite++;
                        }
                    }
                }
                else
                {
                    satellites.SatellitesUsedForFix = null;
                }
                satellites.DilutionOfPrecision = double.Parse(data[15]);
                satellites.HorizontalDilutionOfPrecision = double.Parse(data[16]);
                satellites.VerticalDilutionOfPrecision = double.Parse(data[17]);
                OnActiveSatellitesReceived(this, satellites);
            }
        }

        #endregion NMEADecoder methods & properties
    }
}