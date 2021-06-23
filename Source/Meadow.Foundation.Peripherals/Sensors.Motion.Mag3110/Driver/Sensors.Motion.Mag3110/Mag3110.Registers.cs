using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Mag3110
    {
        /// <summary>
        /// Register addresses in the sensor.
        /// </summary>
        private static class Registers
        {
            public const byte DR_STATUS = 0x00;
            public const byte X_MSB = 0x01;
            public const byte X_LSB = 0x02;
            public const byte Y_MSB = 0x03;
            public const byte Y_LSB = 0x04;
            public const byte Z_MSB = 0x05;
            public const byte Z_LSB = 0x06;
            public const byte WHO_AM_I = 0x07;
            public const byte SYSTEM_MODE = 0x08;
            public const byte X_OFFSET_MSB = 0x09;
            public const byte X_OFFSET_LSB = 0x0a;
            public const byte Y_OFFSET_MSB = 0x0b;
            public const byte Y_OFFSET_LSB = 0x0c;
            public const byte Z_OFFSET_MSB = 0x0d;
            public const byte Z_OFFSET_LSB = 0x0e;
            public const byte TEMPERATURE = 0x0f;
            public const byte CONTROL_1 = 0x10;
            public const byte CONTROL_2 = 0x11;
        }
    }
}
