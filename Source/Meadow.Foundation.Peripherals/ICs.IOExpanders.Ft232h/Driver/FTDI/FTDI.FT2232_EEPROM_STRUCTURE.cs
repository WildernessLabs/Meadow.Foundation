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
    // EEPROM class for FT2232C, FT2232L and FT2232D
    /// <summary>
    /// EEPROM structure specific to FT2232 devices.
    /// Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class FT2232_EEPROM_STRUCTURE : FT_EEPROM_DATA
    {
        //private bool Rev5                   = true;
        //private bool IsoInA                 = false;
        //private bool IsoInB                 = false;
        //private bool IsoOutA                = false;
        //private bool IsoOutB                = false;
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
        /// The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
        /// </summary>
        public UInt16 USBVersion = 0x0200;
        /// <summary>
        /// Enables high current IOs on channel A
        /// </summary>
        public bool AIsHighCurrent = false;
        /// <summary>
        /// Enables high current IOs on channel B
        /// </summary>
        public bool BIsHighCurrent = false;
        /// <summary>
        /// Determines if channel A is in FIFO mode
        /// </summary>
        public bool IFAIsFifo = false;
        /// <summary>
        /// Determines if channel A is in FIFO target mode
        /// </summary>
        public bool IFAIsFifoTar = false;
        /// <summary>
        /// Determines if channel A is in fast serial mode
        /// </summary>
        public bool IFAIsFastSer = false;
        /// <summary>
        /// Determines if channel A loads the VCP driver
        /// </summary>
        public bool AIsVCP = true;
        /// <summary>
        /// Determines if channel B is in FIFO mode
        /// </summary>
        public bool IFBIsFifo = false;
        /// <summary>
        /// Determines if channel B is in FIFO target mode
        /// </summary>
        public bool IFBIsFifoTar = false;
        /// <summary>
        /// Determines if channel B is in fast serial mode
        /// </summary>
        public bool IFBIsFastSer = false;
        /// <summary>
        /// Determines if channel B loads the VCP driver
        /// </summary>
        public bool BIsVCP = true;
    }
}
