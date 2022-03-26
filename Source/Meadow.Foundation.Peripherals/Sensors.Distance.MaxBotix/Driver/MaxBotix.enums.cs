namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x70
            /// </summary>
            Address_0x70 = 0x70,//112 
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x70
        }

        /// <summary>
        /// Communication used to get data from sensor
        /// Note: some hardware support multiple communication types
        /// </summary>
        public enum CommunicationType
        {
            /// <summary>
            /// Analog - relative to VCC
            /// </summary>
            Analog,
            /// <summary>
            /// Serial 
            /// </summary>
            Serial,
            /// <summary>
            /// I2C
            /// </summary>
            I2C,
        }

        /// <summary>
        /// Maxbotic sensor product types
        /// </summary>
        public enum SensorType
        {
            /// <summary>
            /// LV series - returns data in inches
            /// </summary>
            LV, //inches (Vcc/512) per inch
            /// <summary>
            /// XL series - metric
            /// </summary>
            XL, //cm (Vcc/1024) per cm
            /// <summary>
            /// XL long range - metric
            /// </summary>
            XLLongRange, //cm (Vcc/512) per cm
            /// <summary>
            /// HR series 5 meter - metric
            /// </summary>
            HR5Meter, //mm (Vcc/5120) per mm
            /// <summary>
            /// HR series 10 meter - metric
            /// </summary>
            HR10Meter, //mm (Vcc/10240) per mm
        }
    }
}