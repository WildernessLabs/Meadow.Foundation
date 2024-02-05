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
    // Device type identifiers for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
    /// <summary>
    /// List of FTDI device types
    /// </summary>
    public enum FT_DEVICE
    {
        /// <summary>
        /// FT232B or FT245B device
        /// </summary>
        FT_DEVICE_BM = 0,
        /// <summary>
        /// FT8U232AM or FT8U245AM device
        /// </summary>
        FT_DEVICE_AM, /// 1 
                      /// <summary>
                      /// FT8U100AX device
                      /// </summary>
        FT_DEVICE_100AX,
        /// <summary>
        /// Unknown device
        /// </summary>
        FT_DEVICE_UNKNOWN,
        /// <summary>
        /// FT2232 device
        /// </summary>
        FT_DEVICE_2232,
        /// <summary>
        /// FT232R or FT245R device
        /// </summary>
        FT_DEVICE_232R, /// 5
                        /// <summary>
                        /// FT2232H device
                        /// </summary>
        FT_DEVICE_2232H, /// 6
                         /// <summary>
                         /// FT4232H device
                         /// </summary>
        FT_DEVICE_4232H, /// 7
                         /// <summary>
                         /// FT232H device
                         /// </summary>
        FT_DEVICE_232H, /// 8
                        /// <summary>
                        /// FT X-Series device
                        /// </summary>
        FT_DEVICE_X_SERIES, /// 9
                            /// <summary>
                            /// FT4222 hi-speed device Mode 0 - 2 interfaces
                            /// </summary>
        FT_DEVICE_4222H_0, /// 10
                           /// <summary>
                           /// FT4222 hi-speed device Mode 1 or 2 - 4 interfaces
                           /// </summary>
        FT_DEVICE_4222H_1_2, /// 11
                             /// <summary>
                             /// FT4222 hi-speed device Mode 3 - 1 interface
                             /// </summary>
        FT_DEVICE_4222H_3, /// 12
                           /// <summary>
                           /// OTP programmer board for the FT4222.
                           /// </summary>
        FT_DEVICE_4222_PROG, /// 13
                             /// <summary>
                             /// OTP programmer board for the FT900.
                             /// </summary>
        FT_DEVICE_FT900, /// 14
                         /// <summary>
                         /// OTP programmer board for the FT930.
                         /// </summary>
        FT_DEVICE_FT930, /// 15
                         /// <summary>
                         /// Flash programmer board for the UMFTPD3A.
                         /// </summary>
        FT_DEVICE_UMFTPD3A, /// 16
                            /// <summary>
                            /// FT2233HP hi-speed device.
                            /// </summary>
        FT_DEVICE_2233HP, /// 17
                          /// <summary>
                          /// FT4233HP hi-speed device.
                          /// </summary>
        FT_DEVICE_4233HP, /// 18
                          /// <summary>
                          /// FT2233HP hi-speed device.
                          /// </summary>
        FT_DEVICE_2232HP, /// 19
                          /// <summary>
                          /// FT4233HP hi-speed device.
                          /// </summary>
        FT_DEVICE_4232HP, /// 20
                          /// <summary>
                          /// FT233HP hi-speed device.
                          /// </summary>
        FT_DEVICE_233HP, /// 21
                         /// <summary>
                         /// FT232HP hi-speed device.
                         /// </summary>
        FT_DEVICE_232HP, /// 22
                         /// <summary>
                         /// FT2233HA hi-speed device.
                         /// </summary>
        FT_DEVICE_2232HA, /// 23
                          /// <summary>
                          /// FT4233HA hi-speed device.
                          /// </summary>
        FT_DEVICE_4232HA, /// 24
    };
    
}
