namespace FTD2XX;

/// <summary>
/// Permitted bit mode values for FTDI devices.  For use with SetBitMode
/// </summary>
public static class FT_BIT_MODES
{
    /// <summary>
    /// Reset bit mode
    /// </summary>
    public const byte FT_BIT_MODE_RESET = 0x00;

    /// <summary>
    /// Asynchronous bit-bang mode
    /// </summary>
    public const byte FT_BIT_MODE_ASYNC_BITBANG = 0x01;

    /// <summary>
    /// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
    /// </summary>
    public const byte FT_BIT_MODE_MPSSE = 0x02;

    /// <summary>
    /// Synchronous bit-bang mode
    /// </summary>
    public const byte FT_BIT_MODE_SYNC_BITBANG = 0x04;

    /// <summary>
    /// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
    /// </summary>
    public const byte FT_BIT_MODE_MCU_HOST = 0x08;

    /// <summary>
    /// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
    /// </summary>
    public const byte FT_BIT_MODE_FAST_SERIAL = 0x10;

    /// <summary>
    /// CBUS bit-bang mode - only available on FT232R and FT232H
    /// </summary>
    public const byte FT_BIT_MODE_CBUS_BITBANG = 0x20;

    /// <summary>
    /// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
    /// </summary>
    public const byte FT_BIT_MODE_SYNC_FIFO = 0x40;
}
