using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    public partial class Ina260
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Address_0x40 = 0x40,
            /// <summary>
            /// Bus address 0x41
            /// </summary>
            Address_0x41 = 0x41,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x40
        }

        private enum Register : byte
        {
            Config = 0x00,
            Current = 0x01,
            Voltage = 0x02,
            Power = 0x03,
            MaskEnable = 0x06,
            AlertLimit = 0x07,
            ManufacturerID = 0xFE,
            DieID = 0xFF
        }
    }
}
