using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.RTCs;

/// <summary>
/// Represents a Ab0805 real-time clock
/// </summary>
public partial class Ab0805 : II2cPeripheral, IRealTimeClock
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Gets or sets whether the RTC is running
    /// </summary>
    public bool IsRunning
    {
        get
        {
            byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);
            return (control1 & (1 << Control1Bits.STOP)) == 0;
        }
        set
        {
            byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);
            if (value == true)
            {
                control1 = (byte)(control1 & ~(1 << Control1Bits.STOP));
            }
            else
            {
                control1 |= (byte)(1 << Control1Bits.STOP);
            }
            control1 |= (1 << Control1Bits.WRTC);
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, control1);
        }
    }

    /// <summary>
    /// Has the timer ended
    /// </summary>
    public bool HasTimerEnded
    {
        get
        {
            byte status = i2CCommunications.ReadRegister((byte)Registers.STATUS);
            return (status & (1 << StatusBits.TIM)) != 0;
        }
    }

    /// <summary>
    /// Has the alarm triggered
    /// </summary>
    public bool HasAlarmTriggered
    {
        get
        {
            byte status = i2CCommunications.ReadRegister((byte)Registers.STATUS);
            return (status & (1 << StatusBits.ALM)) != 0;
        }
    }

    private I2cCommunications i2CCommunications;

    /// <summary>
    /// Creates a new Ab0805 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public Ab0805(II2cBus i2cBus)
    {
        i2CCommunications = new I2cCommunications(i2cBus, (byte)Addresses.Default, 20);
        Initialize();
    }

    private void Initialize()
    {
        i2CCommunications.WriteRegister((byte)Registers.CONTROL1, 0x1B);
        i2CCommunications.WriteRegister((byte)Registers.CONFIG_KEY, 0xA1);
        i2CCommunications.WriteRegister((byte)Registers.OSC_CONTROL, 0x08);
    }

    /// <summary>
    /// Gets the current time from the RTC.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/> object representing the current time.</returns>
    public DateTimeOffset GetTime()
    {
        byte[] timeData = new byte[7];
        i2CCommunications.ReadRegister((byte)Registers.SECONDS, timeData);

        int sec = BcdToDecimalWithMask((byte)(timeData[0] & 0x7F), 0x70);
        int min = BcdToDecimalWithMask((byte)(timeData[1] & 0x7F), 0x70);
        int hour = BcdToDecimalWithMask((byte)(timeData[2] & 0x3F), 0x30);
        int day = BcdToDecimalWithMask((byte)(timeData[3] & 0x3F), 0x30);
        int month = BcdToDecimalWithMask((byte)(timeData[4] & 0x1F), 0x10);
        int year = 2000 + BcdToDecimal(timeData[5]);

        if (year < 2000 || year > 2099 || month < 1 || month > 12 || day < 1 || hour < 0 || hour > 23 || min < 0 || min > 59 || sec < 0 || sec > 59)
        {
            throw new Exception("Invalid time data from RTC");
        }
        if (day > DateTime.DaysInMonth(year, month))
        {
            throw new Exception("Invalid date data from RTC");
        }

        return new DateTimeOffset(year, month, day, hour, min, sec, TimeSpan.Zero);
    }

    /// <summary>
    /// Sets the time on the RTC.
    /// </summary>
    /// <param name="time">The <see cref="DateTimeOffset"/> to set.</param>
    public void SetTime(DateTimeOffset time)
    {
        byte control1 = i2CCommunications.ReadRegister((byte)Registers.CONTROL1);

        if ((control1 & (1 << Control1Bits.HourFormat_12_24)) != 0) // If in 12hr mode
        {
            control1 = (byte)(control1 & ~(1 << Control1Bits.HourFormat_12_24)); // Set to 24hr mode
        }

        control1 |= (1 << Control1Bits.WRTC);
        i2CCommunications.WriteRegister((byte)Registers.CONTROL1, control1);

        bool wasRunning = (control1 & (1 << Control1Bits.STOP)) == 0;
        if (wasRunning)
        {
            // Stop the clock before setting time by modifying bit in current interrupts value
            byte stopCommand = (byte)(control1 | (1 << Control1Bits.STOP));
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, stopCommand);
            Thread.Sleep(10);
        }

        if (time.Year < 2000 || time.Year > 2099)
        {
            throw new ArgumentOutOfRangeException(nameof(time), "Year must be between 2000 and 2099.");
        }
        i2CCommunications.WriteRegister((byte)Registers.YEAR, DecimalToBcd(time.Year - 2000));
        i2CCommunications.WriteRegister((byte)Registers.MONTH, DecimalToBcd(time.Month));
        i2CCommunications.WriteRegister((byte)Registers.DATE, DecimalToBcd(time.Day));

        int rtcDow = (int)time.DayOfWeek;
        byte currentDowReg = i2CCommunications.ReadRegister((byte)Registers.DAY_OF_WEEK);
        byte newDowReg = (byte)((currentDowReg & ~DayOfWeekBits.Mask) | (rtcDow & DayOfWeekBits.Mask));
        i2CCommunications.WriteRegister((byte)Registers.DAY_OF_WEEK, newDowReg);

        i2CCommunications.WriteRegister((byte)Registers.HOURS, DecimalToBcd(time.Hour));
        i2CCommunications.WriteRegister((byte)Registers.MINUTES, DecimalToBcd(time.Minute));
        i2CCommunications.WriteRegister((byte)Registers.SECONDS, DecimalToBcd(time.Second)); // CH bit (7) will be 0

        SetHundredths(0);

        if (wasRunning)
        {
            byte startCommand = (byte)((control1 & ~(1 << Control1Bits.STOP)) | (1 << Control1Bits.WRTC));
            i2CCommunications.WriteRegister((byte)Registers.CONTROL1, startCommand);
        }
    }

    /// <summary>
    /// Sets the alarm time
    /// </summary>
    /// <param name="alarmTime">The DateTimeOffset to trigger the alarm</param>
    public void SetAlarm(DateTimeOffset alarmTime)
    {
        SetAlarmTime(alarmTime.DateTime);

        SetAlarmInterrupt(true);

        DisableAlarmRepeat();

        SetAlarmInterruptToControlFOUT();
    }

    void SetAlarmTime(DateTime localTime)
    {
        // Extract time components
        int hundredths = localTime.Millisecond / 10;
        int seconds = localTime.Second;
        int minutes = localTime.Minute;
        int hours = localTime.Hour;
        int date = localTime.Day;
        int month = localTime.Month;
        int dayOfWeek = (int)localTime.DayOfWeek;

        // Convert to BCD format and write to alarm registers
        i2CCommunications.WriteRegister((byte)Registers.ALARM_HUNDREDTHS, DecimalToBcd(hundredths));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_SECONDS, DecimalToBcd(seconds));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_MINUTES, DecimalToBcd(minutes));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_HOURS, DecimalToBcd(hours));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_DATE, DecimalToBcd(date));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_MONTH, DecimalToBcd(month));
        i2CCommunications.WriteRegister((byte)Registers.ALARM_DAY_OF_WEEK, (byte)dayOfWeek);
    }

    void SetAlarmInterruptToControlFOUT()
    {
        byte control2 = i2CCommunications.ReadRegister((byte)Registers.CONTROL2);
        control2 |= 1 << Control2Bits.OUT1S;
        control2 |= 1 << (Control2Bits.OUT1S + 1);
        control2 |= 1 << Control2Bits.OUTPP;
        i2CCommunications.WriteRegister((byte)Registers.CONTROL2, control2);
    }

    void DisableAlarmRepeat()
    {
        byte timerControl = i2CCommunications.ReadRegister((byte)Registers.TIMER_CONTROL);
        byte value = TimerBits.RPT;
        timerControl |= 1 << TimerBits.RPT;
        timerControl &= (byte)~(1 << value + 1);
        timerControl &= (byte)~(1 << value + 2);
        i2CCommunications.WriteRegister((byte)Registers.TIMER_CONTROL, timerControl);
    }

    /// <summary>
    /// Start the timer on the RTC
    /// </summary>
    /// <param name="value">Count down timer value as an integer</param>
    /// <param name="unit">Count down seconds or minutes</param>
    public void StartTimer(byte value, DelayTimeUnit unit = DelayTimeUnit.Seconds)
    {
        if (value < 1 || value > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Timer value must be between 1 and 255.");
        }

        byte timerControl = i2CCommunications.ReadRegister((byte)Registers.TIMER_CONTROL);

        if (unit == DelayTimeUnit.Seconds)
        {
            timerControl |= 1 << TimerBits.TFS;
            value = TimerBits.TFS + 1;
            timerControl &= (byte)~(1 << value);
        }
        else if (unit == DelayTimeUnit.Minutes)
        {
            timerControl |= 1 << TimerBits.TFS;
            timerControl |= 1 << TimerBits.TFS + 1;
        }
        else
        {
            throw new ArgumentException("Invalid time unit specified.", nameof(unit));
        }

        i2CCommunications.WriteRegister((byte)Registers.TIMER_CONTROL, timerControl);

        byte control2 = i2CCommunications.ReadRegister((byte)Registers.CONTROL2);
        value = Control2Bits.OUT2S;
        control2 |= 1 << Control2Bits.OUT2S;
        control2 &= (byte)~(1 << value + 1);
        control2 |= 1 << (Control2Bits.OUT2S + 2);
        control2 |= 1 << Control2Bits.OUTPP;
        //control2 &= (byte)~(1 << value + 2);
        i2CCommunications.WriteRegister((byte)Registers.CONTROL2, control2);

        SetTimerInterrupt(true);
        EnableTimer(true);
    }

    /// <summary>
    /// Reset the timer on the RTC.
    /// </summary>
    public void ResetTimer()
    {
        byte status = i2CCommunications.ReadRegister((byte)Registers.STATUS);
        byte value = StatusBits.TIM;
        status &= (byte)~(1 << value);
        i2CCommunications.WriteRegister((byte)Registers.STATUS, status);
        SetTimerInterrupt(false);
        EnableTimer(false);
    }

    /// <summary>
    /// Reset the alarm on the RTC.
    /// </summary>
    public void ResetAlarm()
    {
        byte status = i2CCommunications.ReadRegister((byte)Registers.STATUS);
        byte value = StatusBits.ALM;
        status &= (byte)~(1 << value);
        i2CCommunications.WriteRegister((byte)Registers.STATUS, status);
        SetAlarmInterrupt(false);
    }

    void EnableTimer(bool enable)
    {
        byte value;
        byte timerControl = i2CCommunications.ReadRegister((byte)Registers.TIMER_CONTROL);
        if (enable)
        {
            timerControl |= 1 << TimerBits.TE;
        }
        else
        {
            value = TimerBits.TE;
            timerControl &= (byte)~(1 << value);
        }
        timerControl |= 1 << TimerBits.TM; //level triggered
        value = TimerBits.TRPT; //don't repeat
        timerControl &= (byte)~(1 << value);
        i2CCommunications.WriteRegister((byte)Registers.TIMER_CONTROL, timerControl);
    }

    void SetTimerInterrupt(bool enable)
    {
        byte interrupts = i2CCommunications.ReadRegister((byte)Registers.INT_MASK);

        if (enable)
        {
            interrupts |= 1 << InterruptMaskBits.TIE;
        }
        else
        {
            var value = InterruptMaskBits.TIE;
            interrupts &= (byte)~(1 << value);
        }
        i2CCommunications.WriteRegister((byte)Registers.INT_MASK, interrupts);
    }

    void SetAlarmInterrupt(bool enable)
    {
        byte interrupts = i2CCommunications.ReadRegister((byte)Registers.INT_MASK);
        byte value;

        if (enable)
        {
            interrupts |= 1 << InterruptMaskBits.AIE;
        }
        else
        {
            value = InterruptMaskBits.AIE;
            interrupts &= (byte)~(1 << value);
        }

        //set interrupts to level (not pulse) 
        value = InterruptMaskBits.IM;
        interrupts &= (byte)~(1 << value);
        interrupts &= (byte)~(1 << value + 1);
        i2CCommunications.WriteRegister((byte)Registers.INT_MASK, interrupts);
    }

    /// <summary>
    /// Reads the hundredths of a second from the RTC.
    /// Note: This is only valid if using an XT oscillator.
    /// </summary>
    /// <returns>Hundredths of a second (0-99).</returns>
    int GetHundredths()
    {
        byte bcdHundredths = i2CCommunications.ReadRegister((byte)Registers.HUNDREDTHS);
        return BcdToDecimal(bcdHundredths);
    }

    /// <summary>
    /// Sets the hundredths of a second on the RTC.
    /// Note: This is only valid if using an XT oscillator.
    /// </summary>
    /// <param name="hundredths">Hundredths of a second (0-99).</param>
    void SetHundredths(int hundredths)
    {
        if (hundredths < 0 || hundredths > 99)
        {
            throw new ArgumentOutOfRangeException(nameof(hundredths), "Hundredths must be between 0 and 99.");
        }
        i2CCommunications.WriteRegister((byte)Registers.HUNDREDTHS, DecimalToBcd(hundredths));
    }

    private byte DecimalToBcd(int value)
    {
        return (byte)(((value / 10) << 4) | (value % 10));
    }

    private int BcdToDecimal(byte bcdValue)
    {
        return ((bcdValue >> 4) * 10) + (bcdValue & 0x0F);
    }

    private int BcdToDecimalWithMask(byte bcdValue, byte tensMask)
    {
        return (bcdValue & 0x0F) + (((bcdValue & tensMask) >> 4) * 10);
    }

    private void RegistersToString()
    {
        Console.WriteLine("Registers:");
        RegisterToString((byte)Registers.STATUS, "Status");
        RegisterToString((byte)Registers.CONTROL1, "Control1");
        RegisterToString((byte)Registers.CONTROL2, "Control2");
        RegisterToString((byte)Registers.INT_MASK, "Interrupt Mask");
        RegisterToString((byte)Registers.TIMER_CONTROL, "Timer Control");
        RegisterToString((byte)Registers.TIMER, "Timer");
        Console.WriteLine();
    }

    private void RegisterToString(byte reg, string name)
    {
        byte value = i2CCommunications.ReadRegister(reg);
        Console.WriteLine($"0x{reg:X2} - {Convert.ToString(value, 2).PadLeft(8, '0')} : {name}");
    }
}