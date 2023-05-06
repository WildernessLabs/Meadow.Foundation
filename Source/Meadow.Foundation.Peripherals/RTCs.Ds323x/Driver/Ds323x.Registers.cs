namespace Meadow.Foundation.RTCs
{
    public partial class Ds323x
    {
        /// <summary>
        /// Register addresses in the sensor
        /// </summary>
        static class Registers
        {
            public static readonly byte Seconds = 0x00;
            public static readonly byte Minutes = 0x01;
            public static readonly byte Hours = 0x02;
            public static readonly byte Day = 0x03;
            public static readonly byte Date = 0x04;
            public static readonly byte Month = 0x05;
            public static readonly byte Year = 0x06;
            public static readonly byte Alarm1Seconds = 0x07;
            public static readonly byte Alarm1Minutes = 0x08;
            public static readonly byte Alarm1Hours = 0x09;
            public static readonly byte Alarm1DayDate = 0x0a;
            public static readonly byte Alarm2Minutes = 0x0b;
            public static readonly byte Alarm2Hours = 0x0c;
            public static readonly byte Alarm2DayDate = 0x0d;
            public static readonly byte Control = 0x0e;
            public static readonly byte ControlStatus = 0x0f;
            public static readonly byte AgingOffset = 0x10;
            public static readonly byte TemperatureMSB = 0x11;
            public static readonly byte TemperatureLSB = 0x12;
        }
    }
}
