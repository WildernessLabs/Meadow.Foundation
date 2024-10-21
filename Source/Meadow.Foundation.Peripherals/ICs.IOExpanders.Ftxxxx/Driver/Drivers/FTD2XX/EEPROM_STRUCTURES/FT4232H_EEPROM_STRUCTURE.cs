namespace FTD2XX;

/// <summary>
/// EEPROM structure specific to FT4232H devices.
/// Inherits from FT_EEPROM_DATA.
/// </summary>
internal class FT4232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
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
    /// Determines if A pins have a slow slew rate
    /// </summary>
    public bool ASlowSlew = false;

    /// <summary>
    /// Determines if the A pins have a Schmitt input
    /// </summary>
    public bool ASchmittInput = false;

    /// <summary>
    /// Determines the A pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte ADriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// Determines if B pins have a slow slew rate
    /// </summary>
    public bool BSlowSlew = false;

    /// <summary>
    /// Determines if the B pins have a Schmitt input
    /// </summary>
    public bool BSchmittInput = false;

    /// <summary>
    /// Determines the B pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte BDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// Determines if C pins have a slow slew rate
    /// </summary>
    public bool CSlowSlew = false;

    /// <summary>
    /// Determines if the C pins have a Schmitt input
    /// </summary>
    public bool CSchmittInput = false;

    /// <summary>
    /// Determines the C pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte CDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// Determines if D pins have a slow slew rate
    /// </summary>
    public bool DSlowSlew = false;

    /// <summary>
    /// Determines if the D pins have a Schmitt input
    /// </summary>
    public bool DSchmittInput = false;

    /// <summary>
    /// Determines the D pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA, FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
    /// </summary>
    public byte DDriveCurrent = FT_DRIVE_CURRENT.FT_DRIVE_CURRENT_4MA;

    /// <summary>
    /// RI of port A acts as RS485 transmit enable (TXDEN)
    /// </summary>
    public bool ARIIsTXDEN = false;

    /// <summary>
    /// RI of port B acts as RS485 transmit enable (TXDEN)
    /// </summary>
    public bool BRIIsTXDEN = false;

    /// <summary>
    /// RI of port C acts as RS485 transmit enable (TXDEN)
    /// </summary>
    public bool CRIIsTXDEN = false;

    /// <summary>
    /// RI of port D acts as RS485 transmit enable (TXDEN)
    /// </summary>
    public bool DRIIsTXDEN = false;

    /// <summary>
    /// Determines if channel A loads the VCP driver
    /// </summary>
    public bool AIsVCP = true;

    /// <summary>
    /// Determines if channel B loads the VCP driver
    /// </summary>
    public bool BIsVCP = true;

    /// <summary>
    /// Determines if channel C loads the VCP driver
    /// </summary>
    public bool CIsVCP = true;

    /// <summary>
    /// Determines if channel D loads the VCP driver
    /// </summary>
    public bool DIsVCP = true;
}
