namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        /// <summary>
        /// The keyboard key state
        /// </summary>
        public enum KeyState
        {
            /// <summary>
            /// Idle
            /// </summary>
            StateIdle = 0,
            /// <summary>
            /// Pressed
            /// </summary>
            StatePress,
            /// <summary>
            /// Long Pressed
            /// </summary>
            StateLongPress,
            /// <summary>
            /// Released
            /// </summary>
            StateRelease
        }

        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x1F
            /// </summary>
            Address_0x1F = 0x1F,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x1F
        }
    }
}