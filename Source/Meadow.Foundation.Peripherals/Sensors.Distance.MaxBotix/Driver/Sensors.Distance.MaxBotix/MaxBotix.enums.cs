namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x70,
            Default = Address0
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