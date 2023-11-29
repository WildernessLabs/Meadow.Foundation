namespace Meadow.Foundation.Sensors.Environmental
{
    partial class Scd4xBase
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x62
            /// </summary>
            Address_0x62 = 0x62,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x62
        }

        /// <summary>
        /// Sdc4x commands
        /// </summary>
        enum Commands : ushort
        {
            //Basic Commands
            StartPeriodicMeasurement = 0x21b1,
            ReadMeasurement = 0xec05, // execution time: 1ms
            StopPeriodicMeasurement = 0x3f86, // execution time: 500ms

            //On-chip output signal compensation
            SetTemperatureOffset = 0x241d, // execution time: 1ms
            GetTemperatureOffset = 0x2318, // execution time: 1ms
            SetSensorAltitude = 0x2427, // execution time: 1ms
            GetSensorAltitude = 0x2322, // execution time: 1ms
            SetAmbientPressure = 0xe000, // execution time: 1ms

            //Field calibration
            PerformForcedCalibration = 0x362f, // execution time: 400ms
            SetAutomaticSelfCalibrationEnabled = 0x2416, // execution time: 1ms
            GetAutomaticSelfCalibrationEnabled = 0x2313, // execution time: 1ms

            //Low power
            StartLowPowerPeriodicMeasurement = 0x21ac,
            GetDataReadyStatus = 0xe4b8, // execution time: 1ms

            //Advanced features
            PersistSettings = 0x3615, // execution time: 800ms
            GetSerialNumber = 0x3682, // execution time: 1ms
            PerformSelfTest = 0x3639, // execution time: 10000ms
            PerformFactoryReset = 0x3632, // execution time: 1200ms
            ReInit = 0x3646, // execution time: 20ms

            //Low power single shot - SCD41 only
            MeasureSingleShot = 0x219d, // execution time: 5000ms
            MeasureSingleShotRhtOnly = 0x2196, // execution time: 50ms
        }
    }
}