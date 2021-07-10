using System;
namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class Mpl115a2
    {
        /// <summary>
        /// Device registers.
        /// </summary>
        private static class Registers
        {
            public static readonly byte PressureMSB = 0x00;
            public static readonly byte PressureLSB = 0x01;
            public static readonly byte TemperatureMSB = 0x02;
            public static readonly byte TemperatureLSB = 0x03;
            public static readonly byte A0MSB = 0x04;
            public static readonly byte A0LSB = 0x05;
            public static readonly byte B1MSB = 0x06;
            public static readonly byte B1LSB = 0x07;
            public static readonly byte B2MSB = 0x08;
            public static readonly byte B2LSB = 0x09;
            public static readonly byte C12MSB = 0x0a;
            public static readonly byte C12LSB = 0x0b;
            public static readonly byte StartConversion = 0x12;
        }
    }
}
