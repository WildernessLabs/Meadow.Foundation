namespace FTD2XX;

/// <summary>
/// Flags that provide information on the FTDI device state
/// </summary>
public static class FT_FLAGS
{
    /// <summary>
    /// Indicates that the device is open
    /// </summary>
    public const int FT_FLAGS_OPENED = 0x00000001;

    /// <summary>
    /// Indicates that the device is enumerated as a hi-speed USB device
    /// </summary>
    public const int FT_FLAGS_HISPEED = 0x00000002;
}
