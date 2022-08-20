namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class As1115
    {
        const byte REG_DIGIT0 = 0x01;
        const byte REG_DIGIT1 = 0x02;
        const byte REG_DIGIT2 = 0x03;
        const byte REG_DIGIT3 = 0x04;
        const byte REG_DIGIT4 = 0x05;
        const byte REG_DIGIT5 = 0x06;
        const byte REG_DIGIT6 = 0x07;
        const byte REG_DIGIT7 = 0x08;

        const byte REG_DECODE_MODE = 0x09;
        const byte REG_GLOBAL_INTEN = 0x0A;
        const byte REG_SCAN_LIMIT = 0x0B;
        const byte REG_SHUTDOWN = 0x0C;
        const byte REG_SELF_ADDR = 0x2D;
        const byte REG_FEATURE = 0x0E;
        const byte REG_DISP_TEST = 0x0F;

        const byte REG_DIGIT01_INTEN = 0x10;
        const byte REG_DIGIT23_INTEN = 0x11;
        const byte REG_DIGIT45_INTEN = 0x12;
        const byte REG_DIGIT67_INTEN = 0x13;

        const byte REG_DIGIT0_DIAG = 0x14;
        const byte REG_DIGIT1_DIAG = 0x15;
        const byte REG_DIGIT2_DIAG = 0x16;
        const byte REG_DIGIT3_DIAG = 0x17;
        const byte REG_DIGIT4_DIAG = 0x18;
        const byte REG_DIGIT5_DIAG = 0x19;
        const byte REG_DIGIT6_DIAG = 0x1A;
        const byte REG_DIGIT7_DIAG = 0x1B;

        const byte REG_KEYA = 0x1C;
        const byte REG_KEYB = 0x1D;

        const byte DECODE_RAW = 0x00;
        const byte DECODE_FONT = 0x01;
        const byte DECODE_ALL_RAW = 0x00;
        const byte DECODE_ALL_FONT = 0xFF;

        const byte FONT_CODEB = 0x00;
        const byte FONT_HEX = 0x01;

        const byte REG_FEATURE_EXTCLK = 0x00;
        const byte REG_FEATURE_RESET = 0x01;
        const byte REG_FEATURE_FONT = 0x02;
        const byte REG_FEATURE_BLINK = 0x04;
        const byte REG_FEATURE_BLINKFREQ = 0x05;
        const byte REG_FEATURE_BLINKSYNC = 0x06;
        const byte REG_FEATURE_BLINKSTART = 0x07;

        const byte REG_SHUTDOWN_SHUTDOWN = 0x00;
        const byte REG_SHUTDOWN_RUNNING = 0x01;
        const byte REG_SHUTDOWN_RESET_FEATUREREG = 0x00;
        const byte REG_SHUTDOWN_PRESERVE_FEATUREREG = 0x80;

        const byte DP_OFF = 0x00;
        const byte DP_ON = 0x01;
    }
}