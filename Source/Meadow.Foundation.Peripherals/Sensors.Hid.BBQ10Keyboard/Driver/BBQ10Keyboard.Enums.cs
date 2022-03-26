namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        public enum KeyState
        {
            StateIdle = 0,
            StatePress,
            StateLongPress,
            StateRelease
        }

        /// <summary>
        ///     Valid addresses for the sensor.
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
