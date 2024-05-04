using Meadow.Hardware;
using System;

namespace Meadow.Foundation.RTCs;

/// <summary>
/// Represents a PCF8523 real-time clock
/// </summary>
public partial class Pcf8523 : II2cPeripheral, IRealTimeClock
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private const int OriginYear = 1980;
    private readonly II2cBus i2cBus;
    private byte[] txBuffer = new byte[20];
    private byte[] rxBuffer = new byte[20];

    /// <summary>
    /// Creates a new Pcf8523 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    public Pcf8523(II2cBus i2cBus)
    {
        this.i2cBus = i2cBus;
        Initialize();
    }

    private void Initialize()
    {
        // put the device into 24-hour mode
        txBuffer[0] = (byte)Registers.Control_1;
        i2cBus.Read((byte)Addresses.Default, rxBuffer.AsSpan()[..1]);
        txBuffer[0] = (byte)Registers.Control_1;
        txBuffer[1] = (byte)(rxBuffer[0] & ~(1 << 3));
        i2cBus.Write((byte)Addresses.Default, txBuffer[..1]);
    }

    /// <inheritdoc/>
    public bool IsRunning
    {
        get
        {
            txBuffer[0] = (byte)Registers.Control_1;
            i2cBus.Read((byte)Addresses.Default, rxBuffer.AsSpan()[..1]);
            return (rxBuffer[0] & (1 << 5)) == 0;
        }
        set
        {
            txBuffer[0] = (byte)Registers.Control_1;
            i2cBus.Read((byte)Addresses.Default, rxBuffer.AsSpan()[..1]);
            txBuffer[1] = (byte)(rxBuffer[0] & (1 << 5));
            i2cBus.Write((byte)Addresses.Default, rxBuffer.AsSpan()[..2]);
        }
    }

    /// <inheritdoc/>
    public DateTimeOffset GetTime()
    {
        // read 10 bytes
        // set the register pointer
        txBuffer[0] = (byte)Registers.Control_1;
        i2cBus.Write((byte)Addresses.Default, txBuffer[..1]);
        i2cBus.Read((byte)Addresses.Default, rxBuffer.AsSpan()[..10]);

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

        return new DateTime(y, m, d, h, min, s);
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
        i2cBus.Write((byte)Addresses.Default, txBuffer.AsSpan()[0..8]);
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