using System;
using System.Collections.Generic;

namespace Sensors.Location.MediaTek
{
    public static class Lookups
    {
        public static Dictionary<string, string> KnownPacketTypes { get; } = new Dictionary<string, string>();

        static Lookups()
        {
            PopulateKnownPacketTypes();
        }

        static void PopulateKnownPacketTypes()
        {
            KnownPacketTypes.Add("001", "PMTK_ACK");
            KnownPacketTypes.Add("010", "PMTK_SYS_MSG");
            KnownPacketTypes.Add("011", "PMTK_TXT_MSG");
            KnownPacketTypes.Add("101", "PMTK_CMD_HOT_START");
            KnownPacketTypes.Add("102", "PMTK_CMD_WARM_START");
            KnownPacketTypes.Add("103", "PMTK_CMD_COLD_START");
            KnownPacketTypes.Add("104", "PMTK_CMD_FULL_COLD_START");
            KnownPacketTypes.Add("220", "PMTK_SET_NMEA_UPDATERATE");
            KnownPacketTypes.Add("251", "PMTK_SET_NMEA_BAUDRATE");
            KnownPacketTypes.Add("301", "PMTK_API_SET_DGPS_MODE");
            KnownPacketTypes.Add("401", "PMTK_API_Q_DGPS_MODE");
            KnownPacketTypes.Add("501", "PMTK_API_DT_DGPS_MODE");
            KnownPacketTypes.Add("313", "PMTK_API_SET_SBAS_ENABLED");
            KnownPacketTypes.Add("413", "PMTK_API_Q_SBAS_ENABLED");
            KnownPacketTypes.Add("513", "PMTK_DT_SBAS_ENABLED");
            KnownPacketTypes.Add("314", "PMTK_API_SET_NMEA_OUTPUT");
            KnownPacketTypes.Add("414", "PMTK_API_Q_NMEA_OUTPUT");
            KnownPacketTypes.Add("514", "PMTK_API_DT_NMEA_OUTPUT");
            KnownPacketTypes.Add("319", "PMTK_API_SET_SBAS_Mode");
            KnownPacketTypes.Add("419", "PMTK_API_Q_SBAS_Mode");
            KnownPacketTypes.Add("519", "PMTK_API_DT_SBAS_Mode");
            KnownPacketTypes.Add("605", "PMTK_Q_RELEASE");
            KnownPacketTypes.Add("705", "PMTK_DT_RELEASE");
            KnownPacketTypes.Add("607", "PMTK_Q_EPO_INFO");
            KnownPacketTypes.Add("707", "PMTK_DT_EPO_INFO");
            KnownPacketTypes.Add("127", "PMTK_CMD_CLEAR_EPO");
            KnownPacketTypes.Add("397", "PMTK_SET_Nav Speed threshold");
            KnownPacketTypes.Add("386", "PMTK_SET_Nav Speed threshold");
            KnownPacketTypes.Add("447", "PMTK_Q_Nav_Threshold");
            KnownPacketTypes.Add("527", "PMTK_DT_Nav_Threshold");
            KnownPacketTypes.Add("161", "PMTK_CMD_STANDBY_MODE");
            KnownPacketTypes.Add("223", "PMTK_SET_AL_DEE_CFG");
            KnownPacketTypes.Add("225", "PMTK_CMD_PERIODIC_MODE");
            KnownPacketTypes.Add("286", "PMTK_CMD_AIC_MODE");
            KnownPacketTypes.Add("869", "PMTK_CMD_EASY_ENABLE");
            KnownPacketTypes.Add("187", "PMTK_LOCUS_CONFIG");
            KnownPacketTypes.Add("330", "PMTK_API_SET_DATUM");
            KnownPacketTypes.Add("430", "PMTK_API_Q_DATUM");
            KnownPacketTypes.Add("530", "PMTK_API_DT_DATUM");
            KnownPacketTypes.Add("351", "PMTK_API_SET_SUPPORT_QZSS_NMEA");
            KnownPacketTypes.Add("352", "PMTK_API_SET_STOP_QZSS");
        }


    }
}
