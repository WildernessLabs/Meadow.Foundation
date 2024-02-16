﻿namespace Meadow.Foundation.ICs.DigiPots
{
    public partial class Ds3502
    {
        /// <summary>
        /// Valid I2C addresses for the sensor
        /// Controlled by pulling A0 and A1 high or low
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x28 (A0 low, A1 low)
            /// </summary>
            Address_0x28 = 0x28,
            /// <summary>
            /// Bus address 0x29 (A0 high, A1 low)
            /// </summary>
            Address_0x29 = 0x29,
            /// <summary>
            /// Bus address 0x2A (A0 low, A1 high)
            /// </summary>
            Address_0x2A = 0x2A,
            /// <summary>
            /// Bus address 0x2B (A0 high, A1 high)
            /// </summary>
            Address_0x2B = 0x2B,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x28
        }

        private enum Register : byte
        {
            /// <summary>
            /// Wiper value register
            /// </summary>
            DS3502_WIPER = 0,
            /// <summary>
            /// Mode selection register (change wiper or set default wiper)
            /// </summary>
            DS3502_MODE = 2,
        }
    }
}