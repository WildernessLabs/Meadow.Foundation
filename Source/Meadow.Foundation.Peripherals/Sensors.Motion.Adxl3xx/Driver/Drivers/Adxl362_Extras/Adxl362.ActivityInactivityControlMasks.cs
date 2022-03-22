using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Adxl362
    {
        /// <summary>
        /// Control bits determining how the activity / inactivity functionality is configured.
        /// </summary>
        protected static class ActivityInactivityControlMasks
        {
            /// <summary>
            /// Determine if the activity functionality is enabled (1) or disabled (0).
            /// </summary>
            public const byte ACTIVITY_ENABLE = 0x01;

            /// <summary>
            /// Determine is activity mode is in reference (1) or absolute mode (0).
            /// </summary>
            public const byte ACTIVITY_MODE = 0x02;

            /// <summary>
            /// Determine if inactivity mode is enabled (1) or disabled (0).
            /// </summary>
            public const byte INACTIVITY_ENABLE = 0x04;

            /// <summary>
            /// Determine is inactivity mode is in reference (1) or absolute mode (0).
            /// </summary>
            public const byte INACTIVITY_MODE = 0x08;

            /// <summary>
            /// Default mode.
            /// </summary>
            /// <remarks>
            /// Activity and inactivity detection are both enabled, and their interrupts
            /// (if mapped) must be acknowledged by the host processor by reading the STATUS
            /// register. Auto-sleep is disabled in this mode. Use this mode for free fall
            /// detection applications.
            /// </remarks>
            public const byte DEFAULT_MODE = 0x00;

            /// <summary>
            /// Link activity and inactivity.
            /// </summary>
            /// <remarks>
            /// Activity and inactivity detection are linked sequentially such that only one
            /// is enabled at a time. Their interrupts (if mapped) must be acknowledged by
            /// the host processor by reading the STATUS register.
            /// </remarks>
            public const byte LINKED_MODE = 0x10;

            /// <summary>
            /// </summary>
            /// <remarks>
            /// Activity and inactivity detection are linked sequentially such that only one is
            /// enabled at a time, and their interrupts are internally acknowledged (do not
            /// need to be serviced by the host processor).
            /// To use either linked or looped mode, both ACT_EN (Bit 0) and INACT_EN (Bit 2)
            /// must be set to 1; otherwise, the default mode is used. For additional information,
            /// refer to the Linking Activity and Inactivity Detection section.
            /// </remarks>
            public const byte LOOP_MODE = 0x30;
        }
    }
}
