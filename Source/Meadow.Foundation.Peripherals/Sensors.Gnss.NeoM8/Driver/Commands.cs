namespace Meadow.Foundation.Sensors.Gnss
{
    public static class Commands
    {
        /// <summary>
        /// All data
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_ALLDATA =
            "$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28";

        public const string PGCMD_ANTENNA = "$PGCMD,33,1*6C";
        public const string PMTK_Q_RELEASE = "$PMTK605*31";
    }
}