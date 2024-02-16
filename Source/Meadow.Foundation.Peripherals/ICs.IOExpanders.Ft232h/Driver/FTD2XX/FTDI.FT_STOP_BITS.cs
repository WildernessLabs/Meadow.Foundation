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
    // Stop Bits
    /// <summary>
    /// Permitted stop bits for FTDI devices
    /// </summary>
    public class FT_STOP_BITS
    {
        /// <summary>
        /// 1 stop bit
        /// </summary>
        public const byte FT_STOP_BITS_1 = 0x00;
        /// <summary>
        /// 2 stop bits
        /// </summary>
        public const byte FT_STOP_BITS_2 = 0x02;
    }
    
}
