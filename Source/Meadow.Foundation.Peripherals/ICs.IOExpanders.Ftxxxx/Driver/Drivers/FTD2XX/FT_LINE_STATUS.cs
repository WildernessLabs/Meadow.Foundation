namespace FTD2XX;

/// <summary>
/// Line status bit definitions
/// </summary>
public static class FT_LINE_STATUS
{
    /// <summary>
    /// Overrun Error (OE) line status
    /// </summary>
    public const byte FT_OE = 0x02;

    /// <summary>
    /// Parity Error (PE) line status
    /// </summary>
    public const byte FT_PE = 0x04;

    /// <summary>
    /// Framing Error (FE) line status
    /// </summary>
    public const byte FT_FE = 0x08;

    /// <summary>
    /// Break Interrupt (BI) line status
    /// </summary>
    public const byte FT_BI = 0x10;
}
