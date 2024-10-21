namespace FTD2XX;

/// <summary>
/// EEPROM structure specific to FT232B and FT245B devices.
/// Inherits from FT_EEPROM_DATA.
/// </summary>
internal class FT232B_EEPROM_STRUCTURE : FT_EEPROM_DATA
{
    /// <summary>
    /// Determines if IOs are pulled down when the device is in suspend
    /// </summary>
    public bool PullDownEnable = false;

    /// <summary>
    /// Determines if the serial number is enabled
    /// </summary>
    public bool SerNumEnable = true;

    /// <summary>
    /// Determines if the USB version number is enabled
    /// </summary>
    public bool USBVersionEnable = true;

    /// <summary>
    /// The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
    /// </summary>
    public short USBVersion = 0x0200;
}

