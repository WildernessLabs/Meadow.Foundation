using System;
namespace Sensors.Location.MediaTek
{
    public struct Commands
    {
        // TODO: consider changing the naming convention of these to CONST case
        // to something more modern.

        //==== Baud Rates
        /// <summary>
        /// 115,200 baud
        /// </summary>
        public const string PMTK_SET_BAUD_115200 = "$PMTK251,115200*1F";
        /// <summary>
        /// 57,600 baud 
        /// </summary>
        public const string PMTK_SET_BAUD_57600 = "$PMTK251,57600*2C";
        /// <summary>
        /// 9,600 baud 
        /// </summary>
        public const string PMTK_SET_BAUD_9600 = "$PMTK251,9600*17";

        //==== Modes
        // TODO: these are just ordinal flags, should be able to build up a command
        // based on the ordinal that each of these gets turned on and use our
        // NmeaSentence class, which will automatically do the checksum and such

        /// <summary>
        /// GPGLL only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_GLLONLY =
            "$PMTK314,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPRMC only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_RMCONLY = 
            "$PMTK314,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPVTG only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_VTGONLY = 
            "$PMTK314,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPGGA only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_GGAONLY = 
            "$PMTK314,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPGSA only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_GSAONLY = 
            "$PMTK314,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPGSV only
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_GSVONLY = 
            "$PMTK314,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// GPRMC + GPGGA
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_RMCGGA =
            "$PMTK314,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*28";
        /// <summary>
        /// GPRMC, GPGGA, + GPGSA
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_RMCGGAGSA =
            "$PMTK314,0,1,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0*29";
        /// <summary>
        /// All data
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_ALLDATA =
            "$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28";
        /// <summary>
        /// None
        /// </summary>
        public const string PMTK_SET_NMEA_OUTPUT_OFF =
            "$PMTK314,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0*28";

        //==== Update Rate
        /**
         Different commands to set the update rate from once a second (1 Hz) to 10 times
         a second (10Hz) Note that these only control the rate at which the position is
         echoed, to actually speed up the position fix you must also send one of the
         position fix rate commands below too. */

        /// <summary>
        /// Once every 10 seconds, 100 millihertz.
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_100_MILLIHERTZ = "$PMTK220,10000*2F";
        /// <summary>
        /// Once every 5 seconds, 200 millihertz.
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_200_MILLIHERTZ = "$PMTK220,5000*1B";
        /// <summary>
        /// 1Hz
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_1HZ = "$PMTK220,1000*1F";
        /// <summary>
        /// 2Hz
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_2HZ = "$PMTK220,500*2B";
        /// <summary>
        /// 5Hz
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_5HZ = "$PMTK220,200*2C";
        /// <summary>
        /// 10Hz
        /// </summary>
        public const string PMTK_SET_NMEA_UPDATE_10HZ = "$PMTK220,100*2F";

        //==== Position fix update rate commands.
        /// <summary>
        /// Once every 10 seconds, 100 millihertz.
        /// </summary>
        public const string PMTK_API_SET_FIX_CTL_100_MILLIHERTZ = "$PMTK300,10000,0,0,0,0*2C";
        /// <summary>
        /// Once every 5 seconds, 200 millihertz.
        /// </summary>
        public const string PMTK_API_SET_FIX_CTL_200_MILLIHERTZ = "$PMTK300,5000,0,0,0,0*18";
        /// <summary>
        /// 1Hz
        /// </summary>
        public const string PMTK_API_SET_FIX_CTL_1HZ = "$PMTK300,1000,0,0,0,0*1C";
        /// <summary>
        /// 5Hz. Note, it can't go any faster than 5x/sec.
        /// </summary>
        public const string PMTK_API_SET_FIX_CTL_5HZ = "$PMTK300,200,0,0,0,0*2F";

        //==== Logging
        /// <summary>
        /// Start logging data
        /// </summary>
        public const string PMTK_LOCUS_STARTLOG = "$PMTK185,0*22";

        /// <summary>
        /// Stop logging data
        /// </summary>
        public const string PMTK_LOCUS_STOPLOG = "$PMTK185,1*23";

        /// <summary>
        /// Acknowledge the start or stop command 
        /// </summary>
        public const string PMTK_LOCUS_STARTSTOPACK = "$PMTK001,185,3*3C";

        /// <summary>
        /// Query the logging status
        /// </summary>
        public const string PMTK_LOCUS_QUERY_STATUS = "$PMTK183*38";

        /// <summary>
        /// Erase the log flash data 
        /// </summary>
        public const string PMTK_LOCUS_ERASE_FLASH = "$PMTK184,1*22";

        // If flash is full, log will overwrite old data with new logs
        public const bool LOCUS_OVERLAP = false;
        // If flash is full, logging will stop
        public const bool LOCUS_FULLSTOP = true;

        /// <summary>
        /// Enable search for SBAS satellite (only works with 1Hz output rate)
        /// </summary>
        public const string PMTK_ENABLE_SBAS = "$PMTK313,1*2E";

        /// <summary>
        /// Use WAAS for DGPS correction data
        /// </summary>
        public const string PMTK_ENABLE_WAAS = "$PMTK301,2*2E";

        /// <summary>
        /// standby command & boot successful message
        /// </summary>
        public const string PMTK_STANDBY = "$PMTK161,0*28";

        /// <summary>
        /// Not needed currently
        /// </summary>
        public const string PMTK_STANDBY_SUCCESS = "$PMTK001,161,3*36";

        /// <summary>
        /// Wake up
        /// </summary>
        public const string PMTK_AWAKE = "$PMTK010,002*2D";

        /// <summary>
        /// ask for the release and version
        /// </summary>
        public const string PMTK_Q_RELEASE = "$PMTK605*31";

        //==== Antenna
        /// <summary>
        /// request for updates on antenna status
        /// </summary>
        public const string PGCMD_ANTENNA = "$PGCMD,33,1*6C";
        /// <summary>
        /// don't show antenna status messages
        /// </summary>
        public const string PGCMD_NOANTENNA = "$PGCMD,33,0*6D";

        //public const string MAXWAITSENTENCE = 10; ///< how long to wait when we're looking for a response
    }
}
