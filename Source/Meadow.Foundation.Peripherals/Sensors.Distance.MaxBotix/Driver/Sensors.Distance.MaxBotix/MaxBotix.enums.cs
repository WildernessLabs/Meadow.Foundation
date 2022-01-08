namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        public enum Address
        {

        }

        public enum CommunicationType
        {
            Analog,
            Serial,
            I2C,
        }

        public enum SensorModel
        {
            //6.54m
            //serial: inches
            MB1000,
            MB1010,
            MB1020,
            MB1030,
            MB1040,

            //5m RS232 or TTL configurable via solder pad
            //serial: mm
            MB1003, 
            MB1013,
            MB1023,
            MB1033,
            MB1043,

            //serial: cm 
            MB1200, //7.6m in cm
            MB1210, //7.6m
            MB1220, //7.6m
            MB1230, //7.6m
            MB1240, //7.6m
            MB1260, //10m
            MB1261, //10m
            MB1300, //7.6m
            MB1310, //7.6m
            MB1320, //7.6m
            MB1330, //7.6m
            MB1340, //7.6m
            MB1360, //10m
            MB1361, //10m

            //5m
            //serial: mm 
            MB1603,
            MB1604,
            MB1613,
            MB1614,
            MB1623,
            MB1624,
            MB1633,
            MB1634,
            MB1643,
            MB1644,

            //16.5m
            //serial: cm
            MB2530,
            MB2532,

            //serial: mm
            MB7360, //5m
            MB7363, //10m
            MB7366, //10m
            MB7367, //5m legacy
            MB7368, //10m
            MB7369, //5m
            MB7375, //1.5m
            MB7380, //5m
            MB7383, //10m
            MB7386, //10m
            MB7387, //5m legacy
            MB7388, //10m
            MB7389, //5m
            MB7395, //1.5m 
        }
    }
}

        //park sensors .... trigger only
        /*
        MB1001,
        MB1002,
        MB1005,
        MB1006,
        MB1007,
        MB1008,
        MB1009,


        MB1003,
        MB1013,
        MB1023,
        MB1033,
        MB1043,
        */

        /* RS232 only
         *        
            MB1004,
            MB1014,
            MB1024,
            MB1034,
            MB1044, */


