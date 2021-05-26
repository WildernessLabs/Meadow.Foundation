using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Ccs811
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x5a,
            Address1 = 0x5b,
            Default = Address0
        }

        private enum Register : byte
        {
            STATUS = 0x00,
            MEAS_MODE = 0x01,
            ALG_RESULT_DATA = 0x02,
            RAW_DATA = 0x03,
            ENV_DATA = 0x05,
            THRESHOLDS = 0x10,
            BASELINE = 0x11,
            HW_ID = 0x20,
            HW_VERSION = 0x21,
            FW_BOOT_VERSION = 0x23,
            FW_APP_VERSION = 0x24,
            INTERNAL_STATE = 0xA0,
            ERROR_ID = 0xE0,
            SW_RESET = 0xFF
        }

        private enum BootloaderCommand : byte
        {
            APP_ERASE = 0xF1,
            APP_DATA = 0xF2,
            APP_VERIFY = 0xF3,
            APP_START = 0xF4,
        }

        public enum MeasurementMode
        {
            /// <summary>
            /// Measurement disabled
            /// </summary>
            Idle = 0 << 4,
            /// <summary>
            /// Constant power mode, IAQ measurement every second
            /// </summary>
            ConstantPower1s = 1 << 4,
            /// <summary>
            /// Pulse heating mode IAQ measurement every 10 seconds
            /// </summary>
            PulseHeat10s = 2 << 4,
            /// <summary>
            /// Low power pulse heating mode IAQ measurement every 60 seconds
            /// </summary>
            LowPower = 3 << 4,
            /// <summary>
            /// Constant power mode, sensor measurement every 250ms
            /// </summary>
            ConstantPower250ms = 4 << 4

        }
    }
}