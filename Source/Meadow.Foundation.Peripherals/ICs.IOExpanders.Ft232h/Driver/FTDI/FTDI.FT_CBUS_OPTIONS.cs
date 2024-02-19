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
    // FT232R CBUS Options
    /// <summary>
    /// Available functions for the FT232R CBUS pins.  Controlled by FT232R EEPROM settings
    /// </summary>
    public class FT_CBUS_OPTIONS
    {
        /// <summary>
        /// FT232R CBUS EEPROM options - Tx Data Enable
        /// </summary>
        public const byte FT_CBUS_TXDEN = 0x00;
        /// <summary>
        /// FT232R CBUS EEPROM options - Power On
        /// </summary>
        public const byte FT_CBUS_PWRON = 0x01;
        /// <summary>
        /// FT232R CBUS EEPROM options - Rx LED
        /// </summary>
        public const byte FT_CBUS_RXLED = 0x02;
        /// <summary>
        /// FT232R CBUS EEPROM options - Tx LED
        /// </summary>
        public const byte FT_CBUS_TXLED = 0x03;
        /// <summary>
        /// FT232R CBUS EEPROM options - Tx and Rx LED
        /// </summary>
        public const byte FT_CBUS_TXRXLED = 0x04;
        /// <summary>
        /// FT232R CBUS EEPROM options - Sleep
        /// </summary>
        public const byte FT_CBUS_SLEEP = 0x05;
        /// <summary>
        /// FT232R CBUS EEPROM options - 48MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK48 = 0x06;
        /// <summary>
        /// FT232R CBUS EEPROM options - 24MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK24 = 0x07;
        /// <summary>
        /// FT232R CBUS EEPROM options - 12MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK12 = 0x08;
        /// <summary>
        /// FT232R CBUS EEPROM options - 6MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK6 = 0x09;
        /// <summary>
        /// FT232R CBUS EEPROM options - IO mode
        /// </summary>
        public const byte FT_CBUS_IOMODE = 0x0A;
        /// <summary>
        /// FT232R CBUS EEPROM options - Bit-bang write strobe
        /// </summary>
        public const byte FT_CBUS_BITBANG_WR = 0x0B;
        /// <summary>
        /// FT232R CBUS EEPROM options - Bit-bang read strobe
        /// </summary>
        public const byte FT_CBUS_BITBANG_RD = 0x0C;
    }
    
}
