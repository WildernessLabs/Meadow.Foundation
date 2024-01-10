namespace Meadow.Foundation.Sensors.Camera;

public partial class Amg8833
{
    internal static class Constants
    {
        public const int StartupDelayMs = 100;
        public const double ThermistorConversion = 0.0625;
        public const int PixelArraySize = 64;
        public const double PixelTempConversion = 0.25;
    }

    internal enum InterruptModes : byte
    {
        Difference = 0x00,
        Absolute = 0x01 << 1
    }

    internal enum Addresses : byte
    {
        Address_0x69 = 0x69,
        Default = Address_0x69
    }

    internal enum Registers : byte
    {
        PCTL = 0x00,
        RST = 0x01,
        FPSC = 0x02,
        INTC = 0x03,
        STAT = 0x04,
        SCLR = 0x05,
        RESERVED = 0x06,
        AVE = 0x07,
        INTHL = 0x08,
        INTHH = 0x09,
        INTLL = 0x0A,
        INTLH = 0x0B,
        IHYSL = 0x0C,
        IHYSH = 0x0D,
        TTHL = 0x0E,
        TTHH = 0x0F,
        INT_OFFSET = 0x010,
        PIXEL_OFFSET = 0x80
    };

    internal enum Modes : byte
    {
        Normal = 0x00,
        Sleep = 0x01,
        StandBy_60 = 0x20,
        StandBy_10 = 0x21
    }

    internal enum Commands : byte
    {
        RST_FlagReset = 0x30,
        RST_InitialReset = 0x3f,
        FPS_One = 0x01,
        FPS_Ten = 0x00,
    }
}
