namespace Meadow.Foundation.Sensors.Accelerometers
{
    public partial class Bmi270
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x68
            /// </summary>
            Address_0x68 = 0x68,
            /// <summary>
            /// Bus address 0x69
            /// </summary>
            Address_0x69 = 0x69,
            /// <summary>
            /// Default bus address 0x68
            /// </summary>
            Default = Address_0x68,
        }

        /// <summary>
        /// The range of acceleration values
        /// Lower values have less range but more precision 
        /// </summary>
        public enum AccelerationRange : byte
        {
            /// <summary>
            /// +/- 2g (gravity)
            /// </summary>
            _2g = 0,
            /// <summary>
            /// +/- 4g (gravity)
            /// </summary>
            _4g = 1,
            /// <summary>
            /// +/- 8g (gravity) - default
            /// </summary>
            _8g = 2,
            /// <summary>
            /// +/- 16g (gravity)
            /// </summary>
            _16g = 3,
        }

        /// <summary>
        /// The range of angular velocity (gyroscopic) values
        /// Lower values have less range but more precision 
        /// </summary>
        public enum AngularVelocityRange : byte
        {
            /// <summary>
            /// +/- 2000 degrees per second - default
            /// </summary>
            _2000dps,
            /// <summary>
            /// +/- 1000 degrees per second
            /// </summary>
            _1000dps,
            /// <summary>
            /// +/- 5000 degrees per second
            /// </summary>
            _500dps,
            /// <summary>
            /// +/- 250 degrees per second
            /// </summary>
            _250dps,
            /// <summary>
            /// +/- 125 degrees per second
            /// </summary>
            _125dps,
        }

        /// <summary>
        /// The device power mode
        /// </summary>
        public enum PowerMode : byte
        {
            /// <summary>
            /// Lowest power mode, will still maintain configuration, no motion sensing
            /// </summary>
            Suspend,
            /// <summary>
            /// Configuration mode, all features accessible, no motion sensing
            /// </summary>
            Configuration,
            /// <summary>
            /// Low power mode, motion sensing at lowest possible power consumption
            /// </summary>
            LowPower,
            /// <summary>
            /// Normal power mode, alias free motion sensing 
            /// </summary>
            Normal,
            /// <summary>
            /// Performance mode, motion sensing at maximum sensor performance
            /// </summary>
            Performance,
        }
    }
}