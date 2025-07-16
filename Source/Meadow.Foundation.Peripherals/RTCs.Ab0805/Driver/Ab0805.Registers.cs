namespace Meadow.Foundation.RTCs;

internal enum Registers : byte
{
    HUNDREDTHS = 0x00,
    SECONDS = 0x01,
    MINUTES = 0x02,
    HOURS = 0x03,
    DATE = 0x04,
    MONTH = 0x05,
    YEAR = 0x06,
    DAY_OF_WEEK = 0x07,
    ALARM_HUNDREDTHS = 0x08,
    ALARM_SECONDS = 0x09,
    ALARM_MINUTES = 0x0A,
    ALARM_HOURS = 0x0B,
    ALARM_DATE = 0x0C,
    ALARM_MONTH = 0x0D,
    ALARM_DAY_OF_WEEK = 0x0E,
    STATUS = 0x0F,
    CONTROL1 = 0x10,
    CONTROL2 = 0x11,
    INT_MASK = 0x12,
    SQW = 0x13,
    CAL_XT = 0x14,
    CAL_RC_HI = 0x15,
    CAL_RC_LO = 0x16,
    SLEEP_CONTROL = 0x17,
    TIMER_CONTROL = 0x18,
    TIMER = 0x19,
    TIMER_INITIAL = 0x1A,
    WDT = 0x1B,
    OSC_CONTROL = 0x1C,
    OSC_STATUS = 0x1D,
    CONFIG_KEY = 0x1F,
    TRICKLE = 0x20,
    BREF_CONTROL = 0x21,
    AF_CONTROL = 0x26,
    BATMODE_IO = 0x27,
    ID0 = 0x28,
    ID1 = 0x29,
    ID2 = 0x2A,
    ID3 = 0x2B,
    ID4 = 0x2C,
    ID5 = 0x2D,
    ID6 = 0x2E,
    ASTAT = 0x2F,
    OCTRL = 0x30,
    EXTENSION_ADDR = 0x3F
}

/// <summary>
/// Status register bit positions
/// </summary>
internal static class StatusBits
{
    public const byte CB = 7;
    public const byte BAT = 6;
    public const byte WDT = 5;
    public const byte BL = 4;
    public const byte TIM = 3; //timer
    public const byte ALM = 2; //alarm          
    public const byte EX2 = 1;
    public const byte EX1 = 0;
}

/// <summary>
/// Control1 register bit positions
/// </summary>
internal static class Control1Bits
{
    public const byte STOP = 7;
    public const byte HourFormat_12_24 = 6; // Hour Format (0 = 24h, 1 = 12h)
    public const byte OUTB = 5;
    public const byte OUT = 4;
    public const byte ARST = 2;           // Auto Reset Enable
    public const byte WRTC = 0;
}

/// <summary>
/// Control2 register bit positions
/// </summary>
internal static class Control2Bits
{
    public const int OUTPP = 7; // Output Pin Polarity
    public const int OUT2S = 2; // 2:4 timer         
    public const int OUT1S = 0; // 0:1 alarm
}

/// <summary>
/// Interrupt Mask register bit positions
/// </summary>
internal static class InterruptMaskBits
{
    public const byte CEB = 7;
    public const byte IM = 5; //5 & 6
    public const byte BLIE = 4;
    public const byte TIE = 3;
    public const byte AIE = 2;
    public const byte EX2E = 1;
    public const byte EX1E = 0;
}

/// <summary>
/// Interrupt Mask register bit positions
/// </summary>
internal static class TimerBits
{
    public const byte TE = 7;
    public const byte TM = 6;
    public const byte TRPT = 5;
    public const byte RPT = 2; //2:4
    public const byte TFS = 0; //0:1
}

internal static class DayOfWeekBits
{
    public const byte Mask = 0x07; // To read bits 2,1,0
}