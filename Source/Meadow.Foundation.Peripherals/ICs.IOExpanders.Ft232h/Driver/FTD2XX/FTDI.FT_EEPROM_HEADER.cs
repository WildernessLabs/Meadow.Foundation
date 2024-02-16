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


using System;
using System.Runtime.InteropServices;


namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FTDI
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct FT_EEPROM_HEADER
    {
        public UInt32 deviceType;		// FTxxxx device type to be programmed
        // Device descriptor options
        public UInt16 VendorId;				// 0x0403
        public UInt16 ProductId;				// 0x6001
        public byte SerNumEnable;			// non-zero if serial number to be used
        // Config descriptor options
        public UInt16 MaxPower;				// 0 < MaxPower <= 500
        public byte SelfPowered;			// 0 = bus powered, 1 = self powered
        public byte RemoteWakeup;			// 0 = not capable, 1 = capable
        // Hardware options
        public byte PullDownEnable;		// non-zero if pull down in suspend enabled
    }
    
}
