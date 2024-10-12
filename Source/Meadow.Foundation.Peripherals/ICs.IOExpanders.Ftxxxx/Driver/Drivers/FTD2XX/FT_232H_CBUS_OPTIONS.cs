namespace FTD2XX;

/// <summary>
/// Available functions for the FT232H CBUS pins.  Controlled by FT232H EEPROM settings
/// </summary>
public static class FT_232H_CBUS_OPTIONS
{
    /// <summary>
    /// FT232H CBUS EEPROM options - Tristate
    /// </summary>
    public const byte FT_CBUS_TRISTATE = 0x00;

    /// <summary>
    /// FT232H CBUS EEPROM options - Rx LED
    /// </summary>
    public const byte FT_CBUS_RXLED = 0x01;

    /// <summary>
    /// FT232H CBUS EEPROM options - Tx LED
    /// </summary>
    public const byte FT_CBUS_TXLED = 0x02;

    /// <summary>
    /// FT232H CBUS EEPROM options - Tx and Rx LED
    /// </summary>
    public const byte FT_CBUS_TXRXLED = 0x03;

    /// <summary>
    /// FT232H CBUS EEPROM options - Power Enable#
    /// </summary>
    public const byte FT_CBUS_PWREN = 0x04;

    /// <summary>
    /// FT232H CBUS EEPROM options - Sleep
    /// </summary>
    public const byte FT_CBUS_SLEEP = 0x05;

    /// <summary>
    /// FT232H CBUS EEPROM options - Drive pin to logic 0
    /// </summary>
    public const byte FT_CBUS_DRIVE_0 = 0x06;

    /// <summary>
    /// FT232H CBUS EEPROM options - Drive pin to logic 1
    /// </summary>
    public const byte FT_CBUS_DRIVE_1 = 0x07;

    /// <summary>
    /// FT232H CBUS EEPROM options - IO Mode
    /// </summary>
    public const byte FT_CBUS_IOMODE = 0x08;

    /// <summary>
    /// FT232H CBUS EEPROM options - Tx Data Enable
    /// </summary>
    public const byte FT_CBUS_TXDEN = 0x09;

    /// <summary>
    /// FT232H CBUS EEPROM options - 30MHz clock
    /// </summary>
    public const byte FT_CBUS_CLK30 = 0x0A;

    /// <summary>
    /// FT232H CBUS EEPROM options - 15MHz clock
    /// </summary>
    public const byte FT_CBUS_CLK15 = 0x0B;

    /// <summary>
    /// FT232H CBUS EEPROM options - 7.5MHz clock
    /// </summary>
    public const byte FT_CBUS_CLK7_5 = 0x0C;
}