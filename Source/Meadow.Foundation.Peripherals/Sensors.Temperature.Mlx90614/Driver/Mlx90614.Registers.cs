namespace Meadow.Foundation.Sensors.Temperature;

public partial class Mlx90614
{
    /// <summary>
    /// LM75 Registers
    /// </summary>
    private enum Registers : byte
    {
        // RAM
        RAWIR1 = 0x04,
        RAWIR2 = 0x05,
        TA = 0x06,
        TOBJ1 = 0x07,
        TOBJ2 = 0x08,
        // EEPROM
        EEPROM_TOMAX = 0x20,
        EEPROM_TOMIN = 0x21,
        EEPROM_PWMCTRL = 0x22,
        EEPROM_TARANGE = 0x23,
        EEPROM_EMISS = 0x24,
        EEPROM_CONFIG = 0x25,
        EEPROM_ADDR = 0x2E,
        EEPROM_ID1 = 0x3C,
        EEPROM_ID2 = 0x3D,
        EEPROM_ID3 = 0x3E,
        EEPROM_ID4 = 0x3F
    }
}
