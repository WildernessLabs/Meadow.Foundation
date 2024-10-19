namespace FTD2XX;

/// <summary>
/// EEPROM structure specific to FT2232 devices.
/// Inherits from FT_EEPROM_DATA.
/// </summary>
internal class FT2232_EEPROM_STRUCTURE : FT_EEPROM_DATA
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

    /// <summary>
    /// Enables high current IOs on channel A
    /// </summary>
    public bool AIsHighCurrent = false;

    /// <summary>
    /// Enables high current IOs on channel B
    /// </summary>
    public bool BIsHighCurrent = false;

    /// <summary>
    /// Determines if channel A is in FIFO mode
    /// </summary>
    public bool IFAIsFifo = false;

    /// <summary>
    /// Determines if channel A is in FIFO target mode
    /// </summary>
    public bool IFAIsFifoTar = false;

    /// <summary>
    /// Determines if channel A is in fast serial mode
    /// </summary>
    public bool IFAIsFastSer = false;

    /// <summary>
    /// Determines if channel A loads the VCP driver
    /// </summary>
    public bool AIsVCP = true;

    /// <summary>
    /// Determines if channel B is in FIFO mode
    /// </summary>
    public bool IFBIsFifo = false;

    /// <summary>
    /// Determines if channel B is in FIFO target mode
    /// </summary>
    public bool IFBIsFifoTar = false;

    /// <summary>
    /// Determines if channel B is in fast serial mode
    /// </summary>
    public bool IFBIsFastSer = false;

    /// <summary>
    /// Determines if channel B loads the VCP driver
    /// </summary>
    public bool BIsVCP = true;
}

