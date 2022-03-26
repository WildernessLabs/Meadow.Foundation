using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        /// <summary>
        /// Channel compensation multipliers used to compensate the four (4) color channels of the Bh1745
        /// </summary>
        public class ChannelMultipliers
        {
            /// <summary>
            /// Multiplier for the red color channel
            /// </summary>
            public double Red { get; set; } = 1;
            /// <summary>
            /// Multiplier for the green color channel
            /// </summary>
            public double Green { get; set; } = 1;
            /// <summary>
            /// Multiplier for the blue color channel
            /// </summary>
            public double Blue { get; set; } = 1;
            /// <summary>
            /// Multiplier for the clear color channel.
            /// </summary>
            public double Clear { get; set; } = 1;
        }
    }
}
