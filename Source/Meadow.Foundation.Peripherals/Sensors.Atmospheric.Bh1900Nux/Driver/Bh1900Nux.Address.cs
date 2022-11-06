namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Bh1900Nux
    {
        /// <summary>
        /// Fault queue 
        /// </summary>
        public enum FaultQueue
        {
            /// <summary>
            /// One (1)
            /// </summary>
            One = 0,
            /// <summary>
            /// Two (2)
            /// </summary>
            Two = 1,
            /// <summary>
            /// Four (4)
            /// </summary>
            Four = 2,
            /// <summary>
            /// Six (6)
            /// </summary>
            Six = 3,
        }

        /// <summary>
        /// Polarity
        /// </summary>
        public enum Polarity
        {
            /// <summary>
            /// Active low
            /// </summary>
            ActiveLow = 0,
            /// <summary>
            /// Active high
            /// </summary>
            ActiveHigh = 1,
        }

        /// <summary>
        /// Sampling mode
        /// </summary>
        public enum MeasurementModes
        {
            /// <summary>
            /// Continuous
            /// </summary>
            Continuous = 0,
            /// <summary>
            /// Single reading
            /// </summary>
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