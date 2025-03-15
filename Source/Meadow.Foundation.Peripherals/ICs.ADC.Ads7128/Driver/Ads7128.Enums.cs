using System;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
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

    internal enum Mode
    {
        NotSet,
        Manual, // reading single AINs
        AutoSequence, // using an AnalogInputArray
        Autonomous // monitoring a voltage and setting an alert
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

    internal enum Opcodes : byte
    {
        RegisterRead = 0b0001_0000,
        RegisterWrite = 0b0000_1000,
        SetBit = 0b0001_1000,
        ClearBit = 0b0010_0000,
        BlockRead = 0b0011_0000,
        BlockWrite = 0b0010_1000
    }

    public enum Oversampling : byte
    {
        Samples_1 = 0b000,
        Samples_2 = 0b001,
        Samples_4 = 0b010,
        Samples_8 = 0b011,
        Samples_16 = 0b100,
        Samples_32 = 0b101,
        Samples_64 = 0b110,
        Samples_128 = 0b111,
    }

    internal enum Registers : byte
    {
        // System Control Registers
        SystemStatus = 0x00,
        GeneralConfig = 0x01,
        DataConfig = 0x02,
        OsrConfig = 0x03,
        OpModeConfig = 0x04,
        PinConfig = 0x05,
        GpioConfig = 0x07,
        GpioDriveConfig = 0x09,
        GpioValueOut = 0x0B,
        GpioValueIn = 0x0D,
        ZCDBlanking = 0x0F,
        SequenceConfig = 0x10,
        ChannelSel = 0x11,
        AutoSequenceChannelSel = 0x12,
        AlertChannelSel = 0x14,
        AlertMap = 0x16,
        AlertPinConfig = 0x17,
        EventFlag = 0x18,
        EventHighFlag = 0x1A,
        EventLowFlag = 0x1C,
        EventRegion = 0x1E,
    }
}