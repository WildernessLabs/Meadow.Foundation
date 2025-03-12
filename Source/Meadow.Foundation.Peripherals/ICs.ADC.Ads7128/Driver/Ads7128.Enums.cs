using System;

namespace Meadow.Foundation.ICs.ADC;

public abstract partial class Ads7128
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x10
        /// </summary>
        Address_0x10 = 0x10,
        /// <summary>
        /// Bus address 0x11
        /// </summary>
        Address_0x11 = 0x11,
        /// <summary>
        /// Bus address 0x12
        /// </summary>
        Address_0x12 = 0x12,
        /// <summary>
        /// Bus address 0x13
        /// </summary>
        Address_0x13 = 0x13,
        /// <summary>
        /// Bus address 0x14
        /// </summary>
        Address_0x14 = 0x14,
        /// <summary>
        /// Bus address 0x15
        /// </summary>
        Address_0x15 = 0x15,
        /// <summary>
        /// Bus address 0x16
        /// </summary>
        Address_0x16 = 0x16,
        /// <summary>
        /// Bus address 0x17
        /// </summary>
        Address_0x17 = 0x17,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x10
    }

    [Flags]
    internal enum Status
    {
        BrownOutReset = 1 << 0,
        CRCError = 1 << 1,
        PowerOnConfigError = 1 << 2,
        AveragingDone = 1 << 3,
        RmsDone = 1 << 4,
        I2CSpeed = 1 << 5,
        SequencerBusy = 1 << 6
    }

    internal enum Registers : byte
    {
        // System Control Registers
        SystemControl = 0x00,
        SequenceStart = 0x01,
        DeviceId = 0x02,

        // Configuration Registers
        GlobalChannelConfig = 0x03,
        OsrConfig = 0x04,
        OpModeConfig = 0x05,
        PinConfig = 0x06,
        GpioDataDirection = 0x07,
        GpioDataIn = 0x08,
        GpioDataOut = 0x09,
        GpioPinConfig = 0x0A,
        SequenceConfig = 0x0B,

        // Channel Configuration Registers
        Channel0Config = 0x10,
        Channel1Config = 0x11,
        Channel2Config = 0x12,
        Channel3Config = 0x13,
        Channel4Config = 0x14,
        Channel5Config = 0x15,
        Channel6Config = 0x16,
        Channel7Config = 0x17,

        // Auto-Sequence Channel Configuration Registers
        AutoSequenceChannel0Config = 0x20,
        AutoSequenceChannel1Config = 0x21,
        AutoSequenceChannel2Config = 0x22,
        AutoSequenceChannel3Config = 0x23,
        AutoSequenceChannel4Config = 0x24,
        AutoSequenceChannel5Config = 0x25,
        AutoSequenceChannel6Config = 0x26,
        AutoSequenceChannel7Config = 0x27,

        // Result Registers
        Channel0Result = 0x30,
        Channel1Result = 0x31,
        Channel2Result = 0x32,
        Channel3Result = 0x33,
        Channel4Result = 0x34,
        Channel5Result = 0x35,
        Channel6Result = 0x36,
        Channel7Result = 0x37,

        // Alert Registers
        AlertConfig = 0x40,
        AlertStatus = 0x41,
        Channel0HighThreshold = 0x42,
        Channel0LowThreshold = 0x43,
        Channel1HighThreshold = 0x44,
        Channel1LowThreshold = 0x45,
        Channel2HighThreshold = 0x46,
        Channel2LowThreshold = 0x47,
        Channel3HighThreshold = 0x48,
        Channel3LowThreshold = 0x49,
        Channel4HighThreshold = 0x4A,
        Channel4LowThreshold = 0x4B,
        Channel5HighThreshold = 0x4C,
        Channel5LowThreshold = 0x4D,
        Channel6HighThreshold = 0x4E,
        Channel6LowThreshold = 0x4F,
        Channel7HighThreshold = 0x50,
        Channel7LowThreshold = 0x51
    }
}