using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// The available ADC gain scaling options for the Bh1745
        /// </summary>
        public enum AdcGainTypes : byte
        {
            X1 = 0x0,
            X2 = 0x1,
            X16 = 0x2
        }
    }
}
