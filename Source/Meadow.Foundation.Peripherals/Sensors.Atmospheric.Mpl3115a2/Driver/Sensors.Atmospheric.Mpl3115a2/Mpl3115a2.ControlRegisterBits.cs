using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl3115a2
    {
        /// <summary>
        /// Byte values for the various masks in the control registers.
        /// </summary>
        /// <remarks>
        /// For further information see section 7.17 of the datasheet.
        /// </remarks>
        private class ControlRegisterBits
        {
            /// <summary>
            ///     Control1 - Device in standby when bit 0 is 0.
            /// </summary>
            public static readonly byte Standby = 0x00;

            /// <summary>
            ///     Control1 - Device in active when bit 0 is set to 1
            /// </summary>
            public static readonly byte Active = 0x01;

            /// <summary>
            ///     Control1 - Initiate a single measurement immediately.
            /// </summary>
            public static readonly byte OneShot = 0x02;

            /// <summary>
            ///     Control1 - Perform a software reset when in standby mode.
            /// </summary>
            public static readonly byte SoftwareResetEnable = 0x04;

            /// <summary>
            ///     Control1 - Set the oversample rate to 1.
            /// </summary>
            public static readonly byte OverSample1 = 0x00;

            /// <summary>
            ///     Control1 - Set the oversample rate to 2.
            /// </summary>
            public static readonly byte OverSample2 = 0x08;

            /// <summary>
            ///     Control1 - Set the oversample rate to 4.
            /// </summary>
            public static readonly byte OverSample4 = 0x10;

            /// <summary>
            ///     Control1 - Set the oversample rate to 8.
            /// </summary>
            public static readonly byte OverSample8 = 0x18;

            /// <summary>
            ///     Control1 - Set the oversample rate to 16.
            /// </summary>
            public static readonly byte OverSample16 = 0x20;

            /// <summary>
            ///     Control1 - Set the oversample rate to 32.
            /// </summary>
            public static readonly byte OverSample32 = 0x28;

            /// <summary>
            ///     Control1 - Set the oversample rate to 64.
            /// </summary>
            public static readonly byte OverSample64 = 0x30;

            /// <summary>
            ///     Control1 - Set the oversample rate to 128.
            /// </summary>
            public static readonly byte OverSample128 = 0x38;

            /// <summary>
            ///     Control1 - Altimeter or Barometer mode (Altimeter = 1, Barometer = 0);
            /// </summary>
            public static readonly byte AlimeterMode = 0x80;
        }
    }
}
