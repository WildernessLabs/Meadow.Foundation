﻿namespace Meadow.Foundation.Sensors.Distance
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

        public enum SensorType
        {
            LV, //inches (Vcc/512) per inch
            XL, //cm (Vcc/1024) per cm
            XLLongRange, //cm (Vcc/512) per cm
            HR5Meter, //mm (Vcc/5120) per mm
            HR10Meter, //mm (Vcc/10240) per mm
        }
    }
}