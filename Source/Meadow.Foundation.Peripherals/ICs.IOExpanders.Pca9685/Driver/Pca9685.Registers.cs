namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    private static class Registers
    {
        public const byte Mode1 = 0x00;
        public const byte Mode2 = 0x01;
        public const byte SubAdr1 = 0x02;
        public const byte SubAdr2 = 0x03;
        public const byte SubAdr3 = 0x04;
        public const byte PreScale = 0xFE;
        public const byte Led0OnL = 0x06;
        public const byte Led0OnH = 0x07;
        public const byte Led0OffL = 0x08;
        public const byte Led0OffH = 0x09;
        public const byte AllLedOnL = 0xFA;
        public const byte AllLedOnH = 0xFB;
        public const byte AllLedOffL = 0xFC;
        public const byte AllLedOffH = 0xFD;
    }
}