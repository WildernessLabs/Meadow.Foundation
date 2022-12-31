namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mmcc56x3
    {
        /// <summary>
        /// Register addresses in the sensor
        /// </summary>
        private static class Registers
        {
            public const byte WHO_AM_I = 0x39;
            public const byte CONTROL_0 = 0x1B;
            public const byte CONTROL_1 = 0x1C;
            public const byte CONTROL_2 = 0x1D;
            public const byte STATUS = 0x18;
            public const byte TEMPERATURE = 0x09;
            public const byte OUT_X_L = 0x00;
            public const byte ODR = 0x1A;
        }
    }
}