namespace Meadow.Foundation.Sensors.Environmental
{
    partial class Scd30Base
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x62
            /// </summary>
            Address_0x61 = 0x61,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x61
        }

        /// <summary>
        /// Register addresses
        /// </summary>
        internal enum RegisterAddresses : ushort
        {
            //Basic Commands
            StartPeriodicMeasurement = 0x0010,
            StopPeriodicMeasurement = 0x0104,
            ReadMeasurement = 0x0300,
            ReadFirmwareVersion = 0xd100,

            //On-chip output signal compensation
            SetTemperatureOffset = 0x5403,
            SetAltitude = 0x5102,

            //Field calibration
            SetForcedRecalibration = 0x5204,
            AutoSelfCalibration = 0x5306,

            //Advanced features
            SetMeasurementInterval = 0x4600,
            IsDataReady = 0x0202,
            SoftRest = 0xd304
        }
    }
}