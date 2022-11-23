namespace Meadow.Foundation.Sensors.LoadCell
{
    public partial class Hx711
    {
        /// <summary>
        /// Analog-to-digital converter selectable gain
        /// </summary>
        public enum AdcGain
        {
            /// <summary>
            /// 128 Gain
            /// </summary>
            Gain128 = 1,
            /// <summary>
            /// 32 Gain
            /// </summary>
            Gain32 = 2,
            /// <summary>
            /// 64 Gain
            /// </summary>
            Gain64 = 3,
        }
    }
}