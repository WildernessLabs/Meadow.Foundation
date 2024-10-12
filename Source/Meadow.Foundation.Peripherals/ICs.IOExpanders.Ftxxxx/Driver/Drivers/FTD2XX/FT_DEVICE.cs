namespace FTD2XX;

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
    FT_DEVICE_AM,

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
    FT_DEVICE_232R,

    /// <summary>
    /// FT2232H device
    /// </summary>
    FT_DEVICE_2232H,

    /// <summary>
    /// FT4232H device
    /// </summary>
    FT_DEVICE_4232H,

    /// <summary>
    /// FT232H device
    /// </summary>
    FT_DEVICE_232H,

    /// <summary>
    /// FT X-Series device
    /// </summary>
    FT_DEVICE_X_SERIES,

    /// <summary>
    /// FT4222 hi-speed device Mode 0 - 2 interfaces
    /// </summary>
    FT_DEVICE_4222H_0,

    /// <summary>
    /// FT4222 hi-speed device Mode 1 or 2 - 4 interfaces
    /// </summary>
    FT_DEVICE_4222H_1_2,

    /// <summary>
    /// FT4222 hi-speed device Mode 3 - 1 interface
    /// </summary>
    FT_DEVICE_4222H_3,

    /// <summary>
    /// OTP programmer board for the FT4222.
    /// </summary>
    FT_DEVICE_4222_PROG,

    /// <summary>
    /// OTP programmer board for the FT900.
    /// </summary>
    FT_DEVICE_FT900,

    /// <summary>
    /// OTP programmer board for the FT930.
    /// </summary>
    FT_DEVICE_FT930,

    /// <summary>
    /// Flash programmer board for the UMFTPD3A.
    /// </summary>
    FT_DEVICE_UMFTPD3A,

    /// <summary>
    /// FT2233HP hi-speed device.
    /// </summary>
    FT_DEVICE_2233HP,

    /// <summary>
    /// FT4233HP hi-speed device.
    /// </summary>
    FT_DEVICE_4233HP,

    /// <summary>
    /// FT2233HP hi-speed device.
    /// </summary>
    FT_DEVICE_2232HP,

    /// <summary>
    /// FT4233HP hi-speed device.
    /// </summary>
    FT_DEVICE_4232HP,

    /// <summary>
    /// FT233HP hi-speed device.
    /// </summary>
    FT_DEVICE_233HP,

    /// <summary>
    /// FT232HP hi-speed device.
    /// </summary>
    FT_DEVICE_232HP,

    /// <summary>
    /// FT2233HA hi-speed device.
    /// </summary>
    FT_DEVICE_2232HA,

    /// <summary>
    /// FT4233HA hi-speed device.
    /// </summary>
    FT_DEVICE_4232HA,
};
