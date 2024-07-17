namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    private static class Mode1
    {
        //# Bits
        public const byte Restart = 1 << 7;
        public const byte AutoIncrement = 1 << 5;
        public const byte Sleep = 1 << 4;
        public const byte AllCall = 1 << 0;
    }
}