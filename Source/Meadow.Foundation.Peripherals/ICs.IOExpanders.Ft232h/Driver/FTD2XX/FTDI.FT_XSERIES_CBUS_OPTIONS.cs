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
    /// <summary>
    /// Available functions for the X-Series CBUS pins.  Controlled by X-Series EEPROM settings
    /// </summary>
    public class FT_XSERIES_CBUS_OPTIONS
    {
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Tristate
        /// </summary>
        public const byte FT_CBUS_TRISTATE = 0x00;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - RxLED#
        /// </summary>
        public const byte FT_CBUS_RXLED = 0x01;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - TxLED#
        /// </summary>
        public const byte FT_CBUS_TXLED = 0x02;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - TxRxLED#
        /// </summary>
        public const byte FT_CBUS_TXRXLED = 0x03;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - PwrEn#
        /// </summary>
        public const byte FT_CBUS_PWREN = 0x04;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Sleep#
        /// </summary>
        public const byte FT_CBUS_SLEEP = 0x05;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Drive_0
        /// </summary>
        public const byte FT_CBUS_Drive_0 = 0x06;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Drive_1
        /// </summary>
        public const byte FT_CBUS_Drive_1 = 0x07;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - GPIO
        /// </summary>
        public const byte FT_CBUS_GPIO = 0x08;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - TxdEn
        /// </summary>
        public const byte FT_CBUS_TXDEN = 0x09;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Clk24MHz
        /// </summary>
        public const byte FT_CBUS_CLK24MHz = 0x0A;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Clk12MHz
        /// </summary>
        public const byte FT_CBUS_CLK12MHz = 0x0B;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Clk6MHz
        /// </summary>
        public const byte FT_CBUS_CLK6MHz = 0x0C;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - BCD_Charger
        /// </summary>
        public const byte FT_CBUS_BCD_Charger = 0x0D;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - BCD_Charger#
        /// </summary>
        public const byte FT_CBUS_BCD_Charger_N = 0x0E;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - I2C_TXE#
        /// </summary>
        public const byte FT_CBUS_I2C_TXE = 0x0F;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - I2C_RXF#
        /// </summary>
        public const byte FT_CBUS_I2C_RXF = 0x10;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - VBUS_Sense
        /// </summary>
        public const byte FT_CBUS_VBUS_Sense = 0x11;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - BitBang_WR#
        /// </summary>
        public const byte FT_CBUS_BitBang_WR = 0x12;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - BitBang_RD#
        /// </summary>
        public const byte FT_CBUS_BitBang_RD = 0x13;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Time_Stampe
        /// </summary>
        public const byte FT_CBUS_Time_Stamp = 0x14;
        /// <summary>
        /// FT X-Series CBUS EEPROM options - Keep_Awake#
        /// </summary>
        public const byte FT_CBUS_Keep_Awake = 0x15;
    }
    
}
