using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Bh1745
    {
        public static class Addresses
        {
            /// <summary>
            /// Address of the peripheral when the address pin is pulled low.
            /// </summary>
            public const byte Low = 0x38;

            /// <summary>
            /// Address of the peripheral when the address pin is pulled high.
            /// </summary>
            public const byte High = 0x39;
        }
    }
}
