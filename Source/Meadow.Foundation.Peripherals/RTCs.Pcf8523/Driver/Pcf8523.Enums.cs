namespace Meadow.Foundation.RTCs;

internal enum Registers : byte
{
    Control_1 = 0x00,
    Control_2 = 0x01,
    Control_3 = 0x02,
    Seconds = 0x03,
    Minutes = 0x04,
    Hours = 0x05,
    Days = 0x06,
    Weekdays = 0x07,
    Months = 0x08,
    Years = 0x09,
    MinuteAlarm = 0x0A,
    HourAlarm = 0x0B,
    DayAlarm = 0x0C,
    Weekday_Alarm = 0x0D,
    Offset = 0x0E,
    Tmr_CLKOUT_ctrl = 0x0F,
    Tmr_A_freq_ctrl = 0x10,
    Tmr_A_reg = 0x11,
    Tmr_B_freq_ctrl = 0x12,
    Tmr_B_reg = 0x13,
}

public partial class Pcf8523
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x68
        /// </summary>
        Address_0x68 = 0x68,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x68
    }
}