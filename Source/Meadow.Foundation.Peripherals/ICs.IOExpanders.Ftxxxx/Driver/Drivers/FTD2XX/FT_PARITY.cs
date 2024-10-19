namespace FTD2XX;

/// <summary>
/// Permitted parity values for FTDI devices
/// </summary>
public static class FT_PARITY
{
    /// <summary>
    /// No parity
    /// </summary>
    public const byte FT_PARITY_NONE = 0x00;

    /// <summary>
    /// Odd parity
    /// </summary>
    public const byte FT_PARITY_ODD = 0x01;

    /// <summary>
    /// Even parity
    /// </summary>
    public const byte FT_PARITY_EVEN = 0x02;

    /// <summary>
    /// Mark parity
    /// </summary>
    public const byte FT_PARITY_MARK = 0x03;

    /// <summary>
    /// Space parity
    /// </summary>
    public const byte FT_PARITY_SPACE = 0x04;
}
