using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// Latch behavior types
        /// </summary>
        public enum LatchBehaviorTypes : byte
        {
            /// <summary>
            /// Lach until read or initialized
            /// </summary>
            LatchUntilReadOrInitialized = 0,
            /// <summary>
            /// Latch each measurement
            /// </summary>
            LatchEachMeasurement = 1
        }
    }
}
