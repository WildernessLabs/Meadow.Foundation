namespace FTD2XX;

/// <summary>
/// Permitted flow control values for FTDI devices
/// </summary>
public static class FT_FLOW_CONTROL
{
    /// <summary>
    /// No flow control
    /// </summary>
    public const short FT_FLOW_NONE = 0x0000;

    /// <summary>
    /// RTS/CTS flow control
    /// </summary>
    public const short FT_FLOW_RTS_CTS = 0x0100;

    /// <summary>
    /// DTR/DSR flow control
    /// </summary>
    public const short FT_FLOW_DTR_DSR = 0x0200;

    /// <summary>
    /// Xon/Xoff flow control
    /// </summary>
    public const short FT_FLOW_XON_XOFF = 0x0400;
}
