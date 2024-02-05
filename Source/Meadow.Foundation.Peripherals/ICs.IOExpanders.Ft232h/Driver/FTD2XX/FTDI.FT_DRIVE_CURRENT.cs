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
    // Valid drive current values for FT2232H, FT4232H and FT232H devices
    /// <summary>
    /// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
    /// </summary>
    public class FT_DRIVE_CURRENT
    {
        /// <summary>
        /// 4mA drive current
        /// </summary>
        public const byte FT_DRIVE_CURRENT_4MA = 4;
        /// <summary>
        /// 8mA drive current
        /// </summary>
        public const byte FT_DRIVE_CURRENT_8MA = 8;
        /// <summary>
        /// 12mA drive current
        /// </summary>
        public const byte FT_DRIVE_CURRENT_12MA = 12;
        /// <summary>
        /// 16mA drive current
        /// </summary>
        public const byte FT_DRIVE_CURRENT_16MA = 16;
    }
    
}
