namespace FTD2XX;

/// <summary>
/// EEPROM structure specific to FT232R and FT245R devices.
/// Inherits from FT_EEPROM_DATA.
/// </summary>
internal class FT232R_EEPROM_STRUCTURE : FT_EEPROM_DATA
{
    /// <summary>
    /// Disables the FT232R internal clock source.  
    /// If the device has external oscillator enabled it must have an external oscillator fitted to function
    /// </summary>
    public bool UseExtOsc = false;

    /// <summary>
    /// Enables high current IOs
    /// </summary>
    public bool HighDriveIOs = false;

    /// <summary>
    /// Sets the endpoint size.  This should always be set to 64
    /// </summary>
    public byte EndpointSize = 64;

    /// <summary>
    /// Determines if IOs are pulled down when the device is in suspend
    /// </summary>
    public bool PullDownEnable = false;

    /// <summary>
    /// Determines if the serial number is enabled
    /// </summary>
    public bool SerNumEnable = true;

    /// <summary>
    /// Inverts the sense of the TXD line
    /// </summary>
    public bool InvertTXD = false;

    /// <summary>
    /// Inverts the sense of the RXD line
    /// </summary>
    public bool InvertRXD = false;

    /// <summary>
    /// Inverts the sense of the RTS line
    /// </summary>
    public bool InvertRTS = false;

    /// <summary>
    /// Inverts the sense of the CTS line
    /// </summary>
    public bool InvertCTS = false;

    /// <summary>
    /// Inverts the sense of the DTR line
    /// </summary>
    public bool InvertDTR = false;

    /// <summary>
    /// Inverts the sense of the DSR line
    /// </summary>
    public bool InvertDSR = false;

    /// <summary>
    /// Inverts the sense of the DCD line
    /// </summary>
    public bool InvertDCD = false;

    /// <summary>
    /// Inverts the sense of the RI line
    /// </summary>
    public bool InvertRI = false;

    /// <summary>
    /// Sets the function of the CBUS0 pin for FT232R devices.
    /// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
    /// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
    /// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
    /// </summary>
    public byte Cbus0 = FT_CBUS_OPTIONS.FT_CBUS_SLEEP;

    /// <summary>
    /// Sets the function of the CBUS1 pin for FT232R devices.
    /// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
    /// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
    /// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
    /// </summary>
    public byte Cbus1 = FT_CBUS_OPTIONS.FT_CBUS_SLEEP;

    /// <summary>
    /// Sets the function of the CBUS2 pin for FT232R devices.
    /// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
    /// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
    /// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
    /// </summary>
    public byte Cbus2 = FT_CBUS_OPTIONS.FT_CBUS_SLEEP;

    /// <summary>
    /// Sets the function of the CBUS3 pin for FT232R devices.
    /// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
    /// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
    /// FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
    /// </summary>
    public byte Cbus3 = FT_CBUS_OPTIONS.FT_CBUS_SLEEP;

    /// <summary>
    /// Sets the function of the CBUS4 pin for FT232R devices.
    /// Valid values are FT_CBUS_TXDEN, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED, 
    /// FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12, 
    /// FT_CBUS_CLK6
    /// </summary>
    public byte Cbus4 = FT_CBUS_OPTIONS.FT_CBUS_SLEEP;

    /// <summary>
    /// Determines if the VCP driver is loaded
    /// </summary>
    public bool RIsD2XX = false;
}