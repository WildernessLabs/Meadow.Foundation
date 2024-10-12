namespace FTD2XX;

/// <summary>
/// Modem status bit definitions
/// </summary>
public static class FT_MODEM_STATUS
{
    /// <summary>
    /// Clear To Send (CTS) modem status
    /// </summary>
    public const byte FT_CTS = 0x10;

    /// <summary>
    /// Data Set Ready (DSR) modem status
    /// </summary>
    public const byte FT_DSR = 0x20;

    /// <summary>
    /// Ring Indicator (RI) modem status
    /// </summary>
    public const byte FT_RI = 0x40;

    /// <summary>
    /// Data Carrier Detect (DCD) modem status
    /// </summary>
    public const byte FT_DCD = 0x80;
}