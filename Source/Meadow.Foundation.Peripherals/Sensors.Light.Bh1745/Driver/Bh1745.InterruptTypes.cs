namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// Interrupt types
        /// </summary>
        public enum InterruptTypes : byte
        {
            /// <summary>
            /// Toggle measurement end
            /// </summary>
            ToggleMeasurementEnd = 0x0,
            /// <summary>
            /// Update measurement end
            /// </summary>
            UpdateMeasurementEnd = 0x1,
            /// <summary>
            /// Update consecutive 4x
            /// </summary>
            UpdateConsecutiveX4 = 0x2,
            /// <summary>
            /// Update consecutive 8x
            /// </summary>
            UpdateConsecutiveX8 = 0x3
        }
    }
}
