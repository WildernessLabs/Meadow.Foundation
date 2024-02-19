/*
** FTD2XX_NET.cs
**
** Copyright © 2009-2021 Future Technology Devices International Limited
**
** C# Source file for .NET wrapper of the Windows FTD2XX.dll API calls.
** Main module
**
** Author: FTDI
** Project: CDM Windows Driver Package
** Module: FTD2XX_NET Managed Wrapper
** Requires: 
** Comments:
**
** History:
**  1.0.0	-	Initial version
**  1.0.12	-	Included support for the FT232H device.
**  1.0.14	-	Included Support for the X-Series of devices.
**  1.0.16  -	Overloaded constructor to allow a path to the driver to be passed.
**  1.1.0	-	Handle full 16 character Serial Number and support FT4222 programming board.
**  1.1.2	-	Add new devices and change NULL string for .NET 5 compaibility.

** Ported to NetStandard 2.1 2024, Wilderness Labs
*/

namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FTDI
{
    internal enum FT_OPCODE
    {
        ClockDataBytesOutOnPlusVeClockMSBFirst = 0x10,
        ClockDataBytesOutOnMinusVeClockMSBFirst = 0x11,
        ClockDataBitsOutOnPlusVeClockMSBFirst = 0x12,
        ClockDataBitsOutOnMinusVeClockMSBFirst = 0x13,
        ClockDataBytesInOnPlusVeClockMSBFirst = 0x20,
        ClockDataBytesInOnMinusVeClockMSBFirst = 0x24,
        ClockDataBitsInOnPlusVeClockMSBFirst = 0x22,
        ClockDataBitsInOnMinusVeClockMSBFirst = 0x26,
        ClockDataBytesOutOnMinusBytesInOnPlusVeClockMSBFirst = 0x31,
        ClockDataBytesOutOnPlusBytesInOnMinusVeClockMSBFirst = 0x34,
        ClockDataBitsOutOnMinusBitsInOnPlusVeClockMSBFirst = 0x33,
        ClockDataBitsOutOnPlusBitsInOnMinusVeClockMSBFirst = 0x36,
        ClockDataBytesOutOnPlusVeClockLSBFirst = 0x18,
        ClockDataBytesOutOnMinusVeClockLSBFirst = 0x19,
        ClockDataBitsOutOnPlusVeClockLSBFirst = 0x1A,
        ClockDataBitsOutOnMinusVeClockLSBFirst = 0x1B,
        ClockDataBytesInOnPlusVeClockLSBFirst = 0x28,
        ClockDataBytesInOnMinusVeClockLSBFirst = 0x2C,
        ClockDataBitsInOnPlusVeClockLSBFirst = 0x2A,
        ClockDataBitsInOnMinusVeClockSBFirst = 0x2E,
        ClockDataBytesOutOnMinusBytesInOnPlusVeClockLSBFirst = 0x39,
        ClockDataBytesOutOnPlusBytesInOnMinusVeClockLSBFirst = 0x3C,
        ClockDataBitsOutOnMinusBitsInOnPlusVeClockLSBFirst = 0x3B,
        ClockDataBitsOutOnPlusBitsInOnMinusVeClockLSBFirst = 0x3E,
        ClockDataBytesOutOnPlusVeClockTMSPinLSBFirst = 0x4A,
        ClockDataBytesOutOnMinusVeClockTMSPinSBFirst = 0x4B,
        ClockDataBytesOutOnPlusDataInOnPlusVeClockTMSPinSBFirst = 0x6A,
        ClockDataBytesOutOnMinusDataInOnPlusVeClockTMSPinSBFirst = 0x6B,
        ClockDataBytesOutOnPlusDataInOnMinusVeClockTMSPinSBFirst = 0x6E,
        ClockDataBytesOutOnMinusDataInOnMinusVeClockTMSPinSBFirst = 0x6F,
        SetDataBitsLowByte = 0x80,
        SetDataBitsHighByte = 0x82,
        ReadDataBitsLowByte = 0x81,
        ReadDataBitsHighByte = 0x83,
        ConnectTDItoTDOforLoopback = 0x84,
        DisconnectTDItoTDOforLoopback = 0x85,
        SetTCKSKDivisor = 0x86,
        SetClockDivisor = 0x86,
        CPUModeReadShortAddress = 0x90,
        CPUModeReadExtendedAddress = 0x91,
        CPUModeWriteShortAddress = 0x92,
        CPUModeWriteExtendedAddress = 0x93,
        SendImmediate = 0x87,
        WaitOnIOHigh = 0x88,
        WaitOnIOLow = 0x89,
        DisableClockDivideBy5 = 0x8A,
        EnableClockDivideBy5 = 0x8B,
        Enable3PhaseDataClocking = 0x8C,
        Disable3PhaseDataClocking = 0x8D,
        ClockForNBitsWithNoDataTransfer = 0x8E,
        ClockForNx8BitsWithNoDataTransfer = 0x8F,
        ClockContinuouslyAndWaitOnIOHigh = 0x94,
        ClockContinuouslyAndWaitOnIOLow = 0x95,
        TurnOnAdaptiveClocking = 0x96,
        TurnOffAdaptiveClocking = 0x97,
        ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsHigh = 0x9C,
        ClockForNx8BitsWithNoDataTransferOrUntilGPIOL1IsLow = 0x9D,
        SetIOOnlyDriveOn0AndTristateOn1 = 0x9E,
    }
}
