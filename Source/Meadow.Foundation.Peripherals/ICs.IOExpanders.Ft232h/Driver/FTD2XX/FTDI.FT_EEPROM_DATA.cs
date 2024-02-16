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
    // Base class for EEPROM structures - these elements are common to all devices
    /// <summary>
    /// Common EEPROM elements for all devices.  Inherited to specific device type EEPROMs.
    /// </summary>
    public class FT_EEPROM_DATA
    {
        //private const UInt32 Signature1     = 0x00000000;
        //private const UInt32 Signature2     = 0xFFFFFFFF;
        //private const UInt32 Version        = 0x00000002;
        /// <summary>
        /// Vendor ID as supplied by the USB Implementers Forum
        /// </summary>
        public UInt16 VendorID = 0x0403;
        /// <summary>
        /// Product ID
        /// </summary>
        public UInt16 ProductID = 0x6001;
        /// <summary>
        /// Manufacturer name string
        /// </summary>
        public string Manufacturer = "FTDI";
        /// <summary>
        /// Manufacturer name abbreviation to be used as a prefix for automatically generated serial numbers
        /// </summary>
        public string ManufacturerID = "FT";
        /// <summary>
        /// Device description string
        /// </summary>
        public string Description = "USB-Serial Converter";
        /// <summary>
        /// Device serial number string
        /// </summary>
        public string SerialNumber = "";
        /// <summary>
        /// Maximum power the device needs
        /// </summary>
        public UInt16 MaxPower = 0x0090;
        //private bool PnP                    = true;
        /// <summary>
        /// Indicates if the device has its own power supply (self-powered) or gets power from the USB port (bus-powered)
        /// </summary>
        public bool SelfPowered = false;
        /// <summary>
        /// Determines if the device can wake the host PC from suspend by toggling the RI line
        /// </summary>
        public bool RemoteWakeup = false;
    }
    
}
