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
    /// Type that holds device information for GetDeviceInformation method.
    /// Used with FT_GetDeviceInfo and FT_GetDeviceInfoDetail in FTD2XX.DLL
    /// </summary>
    public class FT_DEVICE_INFO_NODE
    {
        /// <summary>
        /// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
        /// </summary>
        public UInt32 Flags;
        /// <summary>
        /// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        public FT_DEVICE Type;
        /// <summary>
        /// The Vendor ID and Product ID of the device
        /// </summary>
        public UInt32 ID;
        /// <summary>
        /// The physical location identifier of the device
        /// </summary>
        public UInt32 LocId;
        /// <summary>
        /// The device serial number
        /// </summary>
        public string SerialNumber;
        /// <summary>
        /// The device description
        /// </summary>
        public string Description;
        /// <summary>
        /// The device handle.  This value is not used externally and is provided for information only.
        /// If the device is not open, this value is 0.
        /// </summary>
        public IntPtr ftHandle;
    }
    
}
