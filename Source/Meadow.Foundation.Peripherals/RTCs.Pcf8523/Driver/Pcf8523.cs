using Meadow.Hardware;
using System;

namespace Meadow.Foundation.RTCs;

/// <summary>
/// Represents a PCF8523 real-time clock
/// </summary>
public partial class Pcf8523 : II2cPeripheral, IRealTimeClock, IBatteryBackedPeripheral
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <inheritdoc/>
    public bool IsRunning
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_1);
            return (reg & (1 << 5)) == 0;
        }
        set
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_1);
            if (value)
            {
                reg = (byte)(reg & ~(1 << 5));
            }
            else
            {
                reg = (byte)(reg | (1 << 5));
            }
            i2CCommunications.WriteRegister((byte)Registers.Control_1, reg);
        }
    }

    /// <summary>
    /// Gets or sets whether the alarm interrupt is enabled.
    /// </summary>
    public bool IsAlarmInterruptEnabled
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_1);
            // AIE (Alarm Interrupt Enable) is Bit 1 of Control_1
            return (reg & (1 << 1)) != 0;
        }
        set
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_1);
            if (value)
            {
                reg = (byte)(reg | (1 << 1)); // Enable AIE
            }
            else
            {
                reg = (byte)(reg & ~(1 << 1)); // Disable AIE
            }
            i2CCommunications.WriteRegister((byte)Registers.Control_1, reg);
        }
    }

    /// <summary>
    ///  Gets whether timer A is set (indicating a timer event occurred)
    /// </summary>
    public bool HasTimerAInterruptTriggered
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            return (reg & (1 << 6)) != 0;
        }
    }

    /// <summary>
    ///  Gets or sets whether timer B is set (indicating a timer event occurred)
    /// </summary>
    public bool HasTimerBInterruptTriggered
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            return (reg & (1 << 5)) != 0;
        }
    }

    /// <summary>
    /// Gets or sets whether the alarm flag is set (indicating an alarm occurred)
    /// </summary>
    public bool IsAlarmInterruptGenerated
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            return (reg & (1 << 3)) != 0;
        }
    }

    /// <summary>
    /// Gets or sets whether the delay timer interrupt is enabled.
    /// </summary>
    public bool IsTimerAInterruptEnabled
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            return (reg & (1 << 1)) != 0;
        }
        set
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            if (value)
            {
                reg |= (byte)(1 << 1);
            }
            else
            {
                reg = (byte)(reg & ~(1 << 1));
            }
            i2CCommunications.WriteRegister((byte)Registers.Control_2, reg);
        }
    }

    /// <summary>
    /// Gets or sets whether the delay timer interrupt is enabled.
    /// </summary>
    public bool IsTimerBInterruptEnabled
    {
        get
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            return (reg & (1 << 0)) != 0;
        }
        set
        {
            var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
            if (value)
            {
                reg |= (byte)(1 << 0);
            }
            else
            {
                reg = (byte)(reg & ~(1 << 0));
            }
            i2CCommunications.WriteRegister((byte)Registers.Control_2, reg);
        }
    }

    private byte[] txBuffer = new byte[16];
    private byte[] rxBuffer = new byte[16];

    private I2cCommunications i2CCommunications;

    /// <summary>
    /// Creates a new Pcf8523 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public Pcf8523(II2cBus i2cBus)
    {
        i2CCommunications = new I2cCommunications(i2cBus, (byte)Addresses.Default, 20);
        Initialize();
    }

    private void Initialize()
    {
        // put the device into 24-hour mode
        var reg = i2CCommunications.ReadRegister((byte)Registers.Control_1);
        reg = (byte)(reg & ~(1 << 3));
        i2CCommunications.WriteRegister((byte)Registers.Control_1, reg);

        // make sure we're in battery state monitor mode, direct switching on
        i2CCommunications.WriteRegister((byte)Registers.Control_3, 0x20);
    }

    /// <summary>
    /// Reads the battery low indicator register bit
    /// </summary>
    public bool IsBatteryLow()
    {
        var reg = i2CCommunications.ReadRegister((byte)Registers.Control_3);
        var low = (reg & (1 << 2)) != 0;
        return low;
    }

    /// <inheritdoc/>
    public DateTimeOffset GetTime()
    {
        // read 10 bytes
        i2CCommunications.ReadRegister((byte)Registers.Control_1, rxBuffer.AsSpan()[..10]);

        return RTCTimeToDateTimeOffset(rxBuffer, 0x03);
    }

    private DateTimeOffset RTCTimeToDateTimeOffset(Span<byte> rtcRegisters, int startOffset)
    {
        // TODO: check MSB of second register
        // clock integrity is not guaranteed; oscillator has stopped or been interrupted

        var y = FromBCD(rtcRegisters[startOffset + 6]) + 2000;
        var m = FromBCD(rtcRegisters[startOffset + 5]);
        var dow = FromBCD(rtcRegisters[startOffset + 4]);
        var d = FromBCD(rtcRegisters[startOffset + 3]);
        var h = FromBCD(rtcRegisters[startOffset + 2]);
        var min = FromBCD(rtcRegisters[startOffset + 1]);
        var s = FromBCD((byte)(rtcRegisters[startOffset + 0] & 0x7f));

        try
        {
            return new DateTime(y, m, d, h, min, s);
        }
        catch
        {
            return DateTimeOffset.MinValue;
        }
    }

    private void DateTimeOffsetToRTCTime(DateTimeOffset dt, Span<byte> destination, int offset)
    {
        destination[offset + 0] = ToBCD((ushort)dt.Second);
        destination[offset + 1] = ToBCD((ushort)dt.Minute);
        destination[offset + 2] = ToBCD((ushort)dt.Hour);
        destination[offset + 3] = ToBCD((ushort)dt.Day);
        destination[offset + 4] = ToBCD((ushort)(int)dt.DayOfWeek);
        destination[offset + 5] = ToBCD((ushort)dt.Month);
        destination[offset + 6] = ToBCD((ushort)(dt.Year - 2000));
    }

    /// <inheritdoc/>
    public void SetTime(DateTimeOffset time)
    {
        txBuffer[0] = (byte)Registers.Seconds;
        DateTimeOffsetToRTCTime(time, txBuffer, 1);
        i2CCommunications.Write(txBuffer);
    }

    /// <summary>
    /// Sets the alarm time.
    /// </summary>
    /// <param name="alarmTime">The DateTimeOffset to trigger the alarm.</param>
    public void SetAlarm(DateTimeOffset alarmTime)
    {
        byte minute = ToBCD((ushort)alarmTime.Minute);
        byte hour = ToBCD((ushort)alarmTime.Hour);
        byte day = ToBCD((ushort)alarmTime.Day);
        byte weekday = ToBCD((ushort)(int)alarmTime.DayOfWeek);
        byte month = ToBCD((ushort)alarmTime.Month); // Month is not directly used in basic alarm
        byte year = ToBCD((ushort)(alarmTime.Year % 100)); // Year is not directly used in basic alarm

        // Enable all parts of the alarm for an exact match
        byte minuteAlarm = (byte)(minute & 0x7F); // AEN_M = 1
        byte hourAlarm = (byte)(hour & 0x3F);     // AEN_H = 1 (assuming 24-hour mode)
        byte dayAlarm = (byte)(day & 0x3F);       // AEN_D = 1
        byte weekdayAlarm = (byte)(weekday & 0x07); // AEN_W = 1

        i2CCommunications.WriteRegister((byte)Registers.MinuteAlarm, minuteAlarm);
        i2CCommunications.WriteRegister((byte)Registers.HourAlarm, hourAlarm);
        i2CCommunications.WriteRegister((byte)Registers.DayAlarm, dayAlarm);
        i2CCommunications.WriteRegister((byte)Registers.Weekday_Alarm, weekdayAlarm);

        IsAlarmInterruptEnabled = true;
    }

    /// <summary>
    /// Configures the delay interrupt timer A and enables interrupt
    /// </summary>
    /// <param name="value">The delay duration in seconds or minutes (1 - 255)</param>
    /// <param name="unit">The time unit for the delay.</param>
    public void SetTimerA(byte value, DelayTimeUnit unit)
    {
        byte frequencyControl = unit switch
        {
            DelayTimeUnit.Seconds => 0b010,// Select 1 Hz clock for Timer A
            DelayTimeUnit.Minutes => 0b011,// Select 1/60 Hz clock for Timer A
            DelayTimeUnit.Hours => 0b111,// Select 1/3600 Hz clock for Timer A
            _ => throw new ArgumentOutOfRangeException(nameof(unit)),
        };

        i2CCommunications.WriteRegister((byte)Registers.Tmr_A_freq_ctrl, frequencyControl);
        i2CCommunications.WriteRegister((byte)Registers.Tmr_A_reg, value);

        var control2 = i2CCommunications.ReadRegister((byte)Registers.Control_2);
        control2 |= 1 << 1; // Enable Timer A interrupt
        i2CCommunications.WriteRegister((byte)Registers.Control_2, control2);

        var reg = i2CCommunications.ReadRegister((byte)Registers.Tmr_CLKOUT_ctrl);
        reg = (byte)(reg & ~(1 << 7));
        reg |= 0x02;
        i2CCommunications.WriteRegister((byte)Registers.Tmr_CLKOUT_ctrl, reg);
    }

    /// <summary>
    /// Configures the delay interrupt timer B enables interrupt
    /// </summary>
    /// <param name="value">The delay duration in seconds or minutes (1 - 255)</param>
    /// <param name="unit">The time unit for the delay.</param>
    public void SetTimerB(byte value, DelayTimeUnit unit)
    {
        byte frequencyControl = unit switch
        {
            DelayTimeUnit.Seconds => 0b010,// Select 1 Hz clock for Timer B
            DelayTimeUnit.Minutes => 0b011,// Select 1/60 Hz clock for Timer B
            DelayTimeUnit.Hours => 0b111,// Select 1/3600 Hz clock for Timer B
            _ => throw new ArgumentOutOfRangeException(nameof(unit)),
        };

        i2CCommunications.WriteRegister((byte)Registers.Tmr_B_freq_ctrl, frequencyControl);
        i2CCommunications.WriteRegister((byte)Registers.Tmr_B_reg, value);

        var control2 = i2CCommunications.ReadRegister((byte)Registers.Control_2);
        control2 |= 1 << 0; // Enable Timer B interrupt
        i2CCommunications.WriteRegister((byte)Registers.Control_2, control2);

        var reg = i2CCommunications.ReadRegister((byte)Registers.Tmr_CLKOUT_ctrl);
        reg = (byte)(reg & ~(1 << 6));
        reg |= 0x01;
        i2CCommunications.WriteRegister((byte)Registers.Tmr_CLKOUT_ctrl, reg);
    }

    /// <summary>
    /// Clears the alarm and delay interrupt flags
    /// </summary>
    public void ClearFlags()
    {
        var reg = i2CCommunications.ReadRegister((byte)Registers.Control_2);
        reg &= 0x07;
        i2CCommunications.WriteRegister((byte)Registers.Control_2, reg);
    }

    private static byte ToBCD(ushort i)
    {
        return (byte)((i % 10) + ((i / 10) * 0x10));
    }

    private static ushort FromBCD(byte bcd)
    {
        return (ushort)(((bcd) & 0x0F) + (((bcd) >> 4) * 10));
    }
}