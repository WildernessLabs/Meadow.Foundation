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


namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FTDI
{
    /// <summary>
    /// EEPROM structure specific to X-Series devices.
    /// Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class FT_XSERIES_EEPROM_STRUCTURE : FT_EEPROM_DATA
    {
        /// <summary>
        /// Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable = false;
        /// <summary>
        /// Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;
        /// <summary>
        /// Determines if the USB version number is enabled
        /// </summary>
        public bool USBVersionEnable = true;
        /// <summary>
        /// The USB version number: 0x0200 (USB 2.0)
        /// </summary>
        public UInt16 USBVersion = 0x0200;
        /// <summary>
        /// Determines if AC pins have a slow slew rate
        /// </summary>
        public byte ACSlowSlew;
        /// <summary>
        /// Determines if the AC pins have a Schmitt input
        /// </summary>
        public byte ACSchmittInput;
        /// <summary>
        /// Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte ACDriveCurrent;
        /// <summary>
        /// Determines if AD pins have a slow slew rate
        /// </summary>
        public byte ADSlowSlew;
        /// <summary>
        /// Determines if AD pins have a schmitt input
        /// </summary>
        public byte ADSchmittInput;
        /// <summary>
        /// Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte ADDriveCurrent;
        /// <summary>
        /// Sets the function of the CBUS0 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus0;
        /// <summary>
        /// Sets the function of the CBUS1 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus1;
        /// <summary>
        /// Sets the function of the CBUS2 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus2;
        /// <summary>
        /// Sets the function of the CBUS3 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus3;
        /// <summary>
        /// Sets the function of the CBUS4 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus4;
        /// <summary>
        /// Sets the function of the CBUS5 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus5;
        /// <summary>
        /// Sets the function of the CBUS6 pin for FT232H devices.
        /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK24,
        /// FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        /// FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus6;
        /// <summary>
        /// Inverts the sense of the TXD line
        /// </summary>
        public byte InvertTXD;
        /// <summary>
        /// Inverts the sense of the RXD line
        /// </summary>
        public byte InvertRXD;
        /// <summary>
        /// Inverts the sense of the RTS line
        /// </summary>
        public byte InvertRTS;
        /// <summary>
        /// Inverts the sense of the CTS line
        /// </summary>
        public byte InvertCTS;
        /// <summary>
        /// Inverts the sense of the DTR line
        /// </summary>
        public byte InvertDTR;
        /// <summary>
        /// Inverts the sense of the DSR line
        /// </summary>
        public byte InvertDSR;
        /// <summary>
        /// Inverts the sense of the DCD line
        /// </summary>
        public byte InvertDCD;
        /// <summary>
        /// Inverts the sense of the RI line
        /// </summary>
        public byte InvertRI;
        /// <summary>
        /// Determines whether the Battery Charge Detection option is enabled.
        /// </summary>
        public byte BCDEnable;
        /// <summary>
        /// Asserts the power enable signal on CBUS when charging port detected.
        /// </summary>
        public byte BCDForceCbusPWREN;
        /// <summary>
        /// Forces the device never to go into sleep mode.
        /// </summary>
        public byte BCDDisableSleep;
        /// <summary>
        /// I2C slave device address.
        /// </summary>
        public ushort I2CSlaveAddress;
        /// <summary>
        /// I2C device ID
        /// </summary>
        public UInt32 I2CDeviceId;
        /// <summary>
        /// Disable I2C Schmitt trigger.
        /// </summary>
        public byte I2CDisableSchmitt;
        /// <summary>
        /// FT1248 clock polarity - clock idle high (1) or clock idle low (0)
        /// </summary>
        public byte FT1248Cpol;
        /// <summary>
        /// FT1248 data is LSB (1) or MSB (0)
        /// </summary>
        public byte FT1248Lsb;
        /// <summary>
        /// FT1248 flow control enable.
        /// </summary>
        public byte FT1248FlowControl;
        /// <summary>
        /// Enable RS485 Echo Suppression
        /// </summary>
        public byte RS485EchoSuppress;
        /// <summary>
        /// Enable Power Save mode.
        /// </summary>
        public byte PowerSaveEnable;
        /// <summary>
        /// Determines whether the VCP driver is loaded.
        /// </summary>
        public byte IsVCP;
    }
    
}
