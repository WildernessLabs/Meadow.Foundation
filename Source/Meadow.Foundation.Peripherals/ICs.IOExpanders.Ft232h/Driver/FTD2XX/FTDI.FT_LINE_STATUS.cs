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
    // Line Status bits
    /// <summary>
    /// Line status bit definitions
    /// </summary>
    public class FT_LINE_STATUS
    {
        /// <summary>
        /// Overrun Error (OE) line status
        /// </summary>
        public const byte FT_OE = 0x02;
        /// <summary>
        /// Parity Error (PE) line status
        /// </summary>
        public const byte FT_PE = 0x04;
        /// <summary>
        /// Framing Error (FE) line status
        /// </summary>
        public const byte FT_FE = 0x08;
        /// <summary>
        /// Break Interrupt (BI) line status
        /// </summary>
        public const byte FT_BI = 0x10;
    }
    
}
