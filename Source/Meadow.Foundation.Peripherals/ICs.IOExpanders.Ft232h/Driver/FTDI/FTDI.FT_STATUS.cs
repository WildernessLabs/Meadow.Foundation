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
    // Constants for FT_STATUS
    /// <summary>
    /// Status values for FTDI devices.
    /// </summary>
    public enum FT_STATUS
    {
        /// <summary>
        /// Status OK
        /// </summary>
        FT_OK = 0,
        /// <summary>
        /// The device handle is invalid
        /// </summary>
        FT_INVALID_HANDLE,
        /// <summary>
        /// Device not found
        /// </summary>
        FT_DEVICE_NOT_FOUND,
        /// <summary>
        /// Device is not open
        /// </summary>
        FT_DEVICE_NOT_OPENED,
        /// <summary>
        /// IO error
        /// </summary>
        FT_IO_ERROR,
        /// <summary>
        /// Insufficient resources
        /// </summary>
        FT_INSUFFICIENT_RESOURCES,
        /// <summary>
        /// A parameter was invalid
        /// </summary>
        FT_INVALID_PARAMETER,
        /// <summary>
        /// The requested baud rate is invalid
        /// </summary>
        FT_INVALID_BAUD_RATE,
        /// <summary>
        /// Device not opened for erase
        /// </summary>
        FT_DEVICE_NOT_OPENED_FOR_ERASE,
        /// <summary>
        /// Device not poened for write
        /// </summary>
        FT_DEVICE_NOT_OPENED_FOR_WRITE,
        /// <summary>
        /// Failed to write to device
        /// </summary>
        FT_FAILED_TO_WRITE_DEVICE,
        /// <summary>
        /// Failed to read the device EEPROM
        /// </summary>
        FT_EEPROM_READ_FAILED,
        /// <summary>
        /// Failed to write the device EEPROM
        /// </summary>
        FT_EEPROM_WRITE_FAILED,
        /// <summary>
        /// Failed to erase the device EEPROM
        /// </summary>
        FT_EEPROM_ERASE_FAILED,
        /// <summary>
        /// An EEPROM is not fitted to the device
        /// </summary>
        FT_EEPROM_NOT_PRESENT,
        /// <summary>
        /// Device EEPROM is blank
        /// </summary>
        FT_EEPROM_NOT_PROGRAMMED,
        /// <summary>
        /// Invalid arguments
        /// </summary>
        FT_INVALID_ARGS,
        /// <summary>
        /// An other error has occurred
        /// </summary>
        FT_OTHER_ERROR
    };
    
}
