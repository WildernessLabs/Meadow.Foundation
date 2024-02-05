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
    // Bit modes
    /// <summary>
    /// Permitted bit mode values for FTDI devices.  For use with SetBitMode
    /// </summary>
    public class FT_BIT_MODES
    {
        /// <summary>
        /// Reset bit mode
        /// </summary>
        public const byte FT_BIT_MODE_RESET = 0x00;
        /// <summary>
        /// Asynchronous bit-bang mode
        /// </summary>
        public const byte FT_BIT_MODE_ASYNC_BITBANG = 0x01;
        /// <summary>
        /// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FT_BIT_MODE_MPSSE = 0x02;
        /// <summary>
        /// Synchronous bit-bang mode
        /// </summary>
        public const byte FT_BIT_MODE_SYNC_BITBANG = 0x04;
        /// <summary>
        /// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FT_BIT_MODE_MCU_HOST = 0x08;
        /// <summary>
        /// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FT_BIT_MODE_FAST_SERIAL = 0x10;
        /// <summary>
        /// CBUS bit-bang mode - only available on FT232R and FT232H
        /// </summary>
        public const byte FT_BIT_MODE_CBUS_BITBANG = 0x20;
        /// <summary>
        /// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
        /// </summary>
        public const byte FT_BIT_MODE_SYNC_FIFO = 0x40;
    }
    
}
