namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Hmc5883
    {
        /// <summary>
        /// Measurement configuration
        /// </summary>
        public enum MeasurementConfigurations : byte
        {
            /// <summary>
            /// Normal
            /// </summary>
            Normal = 0b_0000_0000,
            /// <summary>
            /// Positive bias
            /// </summary>
            PositiveBias = 0b_0000_0001,
            /// <summary>
            /// Negative bias
            /// </summary>
            NegativeBias = 0b_0000_0010
        }
    }
}