namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Tca95x5
{
    private static class Registers
    {
        public const byte InputPort0 = 0x00;
        public const byte InputPort1 = 0x01;
        public const byte OutputPort0 = 0x02;
        public const byte OutputPort1 = 0x03;
        public const byte PolarityInversionPort0 = 0x04;
        public const byte PolarityInversionPort1 = 0x05;
        public const byte ConfigurationPort0 = 0x06;
        public const byte ConfigurationPort1 = 0x07;
    }
}