namespace FTD2XX;

/// <summary>
/// EEPROM structure specific to FT232H devices.
/// Inherits from FT_EEPROM_DATA.
/// </summary>
internal class FT232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
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
    /// Determines if AC pins have a slow slew rate
    /// </summary>
    public bool ACSlowSlew = false;

    /// <summary>
    /// Determines if the AC pins have a Schmitt input
    /// </summary>
    public bool ACSchmittInput = false;

    /// <summary>
    /// Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte ACDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// Determines if AD pins have a slow slew rate
    /// </summary>
    public bool ADSlowSlew = false;

    /// <summary>
    /// Determines if the AD pins have a Schmitt input
    /// </summary>
    public bool ADSchmittInput = false;

    /// <summary>
    /// Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte ADDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// Sets the function of the CBUS0 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK30,
    /// FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus0 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS1 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN, FT_CBUS_CLK30,
    /// FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus1 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS2 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
    /// </summary>
    public byte Cbus2 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS3 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
    /// </summary>
    public byte Cbus3 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS4 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TXDEN
    /// </summary>
    public byte Cbus4 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS5 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
    /// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus5 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS6 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
    /// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus6 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS7 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE
    /// </summary>
    public byte Cbus7 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS8 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
    /// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus8 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Sets the function of the CBUS9 pin for FT232H devices.
    /// Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
    /// FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
    /// FT_CBUS_TXDEN, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
    /// </summary>
    public byte Cbus9 = FT_232H_CBUS_OPTIONS.FT_CBUS_TRISTATE;

    /// <summary>
    /// Determines if the device is in FIFO mode
    /// </summary>
    public bool IsFifo = false;

    /// <summary>
    /// Determines if the device is in FIFO target mode
    /// </summary>
    public bool IsFifoTar = false;

    /// <summary>
    /// Determines if the device is in fast serial mode
    /// </summary>
    public bool IsFastSer = false;

    /// <summary>
    /// Determines if the device is in FT1248 mode
    /// </summary>
    public bool IsFT1248 = false;

    /// <summary>
    /// Determines FT1248 mode clock polarity
    /// </summary>
    public bool FT1248Cpol = false;

    /// <summary>
    /// Determines if data is ent MSB (0) or LSB (1) in FT1248 mode
    /// </summary>
    public bool FT1248Lsb = false;

    /// <summary>
    /// Determines if FT1248 mode uses flow control
    /// </summary>
    public bool FT1248FlowControl = false;

    /// <summary>
    /// Determines if the VCP driver is loaded
    /// </summary>
    public bool IsVCP = true;

    /// <summary>
    /// For self-powered designs, keeps the FT232H in low power state until ACBUS7 is high
    /// </summary>
    public bool PowerSaveEnable = false;

}
