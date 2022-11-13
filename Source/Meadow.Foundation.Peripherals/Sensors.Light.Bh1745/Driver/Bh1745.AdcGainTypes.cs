namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// The available ADC gain scaling options for the Bh1745
        /// </summary>
        public enum AdcGainTypes : byte
        {
            /// <summary>
            /// 1x scale
            /// </summary>
            X1 = 0x0,
            /// <summary>
            /// 2x scale
            /// </summary>
            X2 = 0x1,
            /// <summary>
            /// 16x scale
            /// </summary>
            X16 = 0x2
        }
    }
}
