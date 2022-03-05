/// <summary>
/// ENUMs for Driver for Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: 2022-03-03
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// -or-
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

namespace Meadow.Foundation.ICs.IOExpanders.Seesaw
{
    /// <summary>
    ///     Valid addresses for the seesaw board
    /// </summary>
    public enum BoardAddresses : byte
    {
        Address0 = 0x49,
        Address1 = 0x4a,
        Address2 = 0x4b,
        Address3 = 0x4c,
        Default = Address0
    }

    public enum BaseAddresses : byte
    {
        Status    = 0x00,
        GPIO      = 0x01,
        Sercom0   = 0x02,

        Timer     = 0x08,
        ADC       = 0x09,
        DAC       = 0x0A,
        Interrupt = 0x0B,
        Dap       = 0x0C,
        Eeprom    = 0x0D,
        Neopixel  = 0x0E,
        Touch     = 0x0F,
        Encoder   = 0x11
    }

    public enum GpioCommands : byte
    {
        DirSetBulk = 0x02,
        DirClrBulk = 0x03,
        Bulk       = 0x04,
        BulkSet    = 0x05,
        BulkClr    = 0x06,
        BulkToggle = 0x07,
        IntenSet   = 0x08,
        IntenClr   = 0x09,
        Intflag    = 0x0A,
        PullenSet  = 0x0B,
        PullenClr  = 0x0C
    }

    public enum StatusCommands : byte
    {
        HwId    = 0x01,
        Version = 0x02,
        Options = 0x03,
        Temp    = 0x04,
        SwReset = 0x7F
    }

    public enum TimerCommands : byte
    {
        Status = 0x00,
        PWM    = 0x01,
        Freq   = 0x02
    }

    public enum AdcCommands : byte
    {
        Status         = 0x00,
        Inten          = 0x02,
        IntenClr       = 0x03,
        WinMode        = 0x04,
        WinThresh      = 0x05,
        ChannelOffset  = 0x07
    }

    public enum SercomCommands : byte
    {
        Status   = 0x00,
        Inten    = 0x02,
        IntenClr = 0x03,
        Baud     = 0x04,
        Data     = 0x05
    }

    public enum NeopixelCommands : byte
    {
        Status     = 0x00,
        Pin        = 0x01,
        Speed      = 0x02,
        BufLength  = 0x03,
        Buf        = 0x04,
        Show       = 0x05
    }

    public enum TouchCommands : byte
    {
        ChannelOffset = 0x10
    }

    public enum EncoderCommands : byte
    {
        Status   = 0x00,
        IntenSet = 0x10,
        IntenClr = 0x20,
        Position = 0x30,
        Delta    = 0x40
    }

    public enum EepromCommands : byte
    {
        I2cAddr = 0x3F
    }

    public enum HwidCodes : byte     // Hardware ID Codes
    {
        ATSAMD09  = 0x55,
        ATtiny8X7 = 0x87
    }

    public enum PinTypes : byte
    {
        Input,
        Output,
        InputPullUp,
        InputPullDown
    }
}