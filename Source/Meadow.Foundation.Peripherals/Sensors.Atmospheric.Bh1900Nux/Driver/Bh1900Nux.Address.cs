namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bh1900Nux
    {
        public enum FaultQueue
        {
            One = 0,
            Two = 1,
            Four = 2,
            Six = 3,
        }

        public enum Polarity
        {
            ActiveLow = 0,
            ActiveHigh = 1,
        }

        public enum Mode
        {
            Continuous = 0,
            Single = 1
        }

        /// <summary>
        /// Valid addresses for the sensor.
        /// </summary>
        public enum Address : byte
        {
            /// <summary>
            /// Bus address 0x48
            /// </summary>
            Address_0x48 = 0x48,
            /// <summary>
            /// Bus address 0x49
            /// </summary>
            Address_0x49 = 0x49,
            /// <summary>
            /// Bus address 0x4a
            /// </summary>
            Address_0x4a = 0x4a,
            /// <summary>
            /// Bus address 0x4b
            /// </summary>
            Address_0x4b = 0x4b,
            /// <summary>
            /// Bus address 0x4c
            /// </summary>
            Address_0x4c = 0x4c,
            /// <summary>
            /// Bus address 0x4d
            /// </summary>
            Address_0x4d = 0x4d,
            /// <summary>
            /// Bus address 0x4e
            /// </summary>
            Address_0x4e = 0x4e,
            /// <summary>
            /// Bus address 0x4f
            /// </summary>
            Address_0x4f = 0x4f,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x48
        }
    }
}