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
    // FT232H CBUS Options
    /// <summary>
    /// Available functions for the FT232H CBUS pins.  Controlled by FT232H EEPROM settings
    /// </summary>
    public class FT_232H_CBUS_OPTIONS
    {
        /// <summary>
        /// FT232H CBUS EEPROM options - Tristate
        /// </summary>
        public const byte FT_CBUS_TRISTATE = 0x00;
        /// <summary>
        /// FT232H CBUS EEPROM options - Rx LED
        /// </summary>
        public const byte FT_CBUS_RXLED = 0x01;
        /// <summary>
        /// FT232H CBUS EEPROM options - Tx LED
        /// </summary>
        public const byte FT_CBUS_TXLED = 0x02;
        /// <summary>
        /// FT232H CBUS EEPROM options - Tx and Rx LED
        /// </summary>
        public const byte FT_CBUS_TXRXLED = 0x03;
        /// <summary>
        /// FT232H CBUS EEPROM options - Power Enable#
        /// </summary>
        public const byte FT_CBUS_PWREN = 0x04;
        /// <summary>
        /// FT232H CBUS EEPROM options - Sleep
        /// </summary>
        public const byte FT_CBUS_SLEEP = 0x05;
        /// <summary>
        /// FT232H CBUS EEPROM options - Drive pin to logic 0
        /// </summary>
        public const byte FT_CBUS_DRIVE_0 = 0x06;
        /// <summary>
        /// FT232H CBUS EEPROM options - Drive pin to logic 1
        /// </summary>
        public const byte FT_CBUS_DRIVE_1 = 0x07;
        /// <summary>
        /// FT232H CBUS EEPROM options - IO Mode
        /// </summary>
        public const byte FT_CBUS_IOMODE = 0x08;
        /// <summary>
        /// FT232H CBUS EEPROM options - Tx Data Enable
        /// </summary>
        public const byte FT_CBUS_TXDEN = 0x09;
        /// <summary>
        /// FT232H CBUS EEPROM options - 30MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK30 = 0x0A;
        /// <summary>
        /// FT232H CBUS EEPROM options - 15MHz clock
        /// </summary>
        public const byte FT_CBUS_CLK15 = 0x0B;/// <summary>
                                               /// FT232H CBUS EEPROM options - 7.5MHz clock
                                               /// </summary>
        public const byte FT_CBUS_CLK7_5 = 0x0C;
    }
}
