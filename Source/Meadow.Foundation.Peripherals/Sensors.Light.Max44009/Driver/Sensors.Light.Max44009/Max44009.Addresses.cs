using System;
namespace Meadow.Foundation.Sensors.Light
{
    public partial class Max44009
    {
        public static class Addresses
        {
            /// <summary>
            /// Address of the peripheral when the address pin is pulled low.
            /// </summary>
            public const byte Low = 0x4A;
            /// <summary>
            /// Address of the peripheral when the address pin is pulled high.
            /// </summary>
            public const byte High = 0x4B;
        }
    }
}
