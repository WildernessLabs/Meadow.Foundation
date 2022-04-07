using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Htu31d
    {
        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x40
            /// </summary>
            Address_0x40 = 0x40,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x40
        }

        private enum Commands : byte
        {
            /** Read temperature and humidity. */
            ReadTempHumidity = 0x00,

            /** Start a conversion! */
            Conversion = 0x40,

            /** Read serial number command. */
            ReadSerial = 0x0A,

           /** Enable heater */
            HeaterOn = 0x04,

            /** Disable heater */
            HeaterOff = 0x02,

           /** Reset command. */
            Reset = 0x1e,
        }
    }
}