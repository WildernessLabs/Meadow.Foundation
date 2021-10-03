namespace Meadow.Foundation.Displays
{
    public partial class Ssd1327
    {
        protected enum Command : byte
        {
            SSD1327_BLACK = 0x0,
            SSD1327_WHITE = 0xF,

            SSD1327_I2C_ADDRESS = 0x3D,

            SSD1305_SETBRIGHTNESS = 0x82,

            SSD1327_SETCOLUMN = 0x15,

            SSD1327_SETROW = 0x75,

            SSD1327_SETCONTRAST = 0x81,

            SSD1305_SETLUT = 0x91,

            SSD1327_SEGREMAP = 0xA0,
            SSD1327_SETSTARTLINE = 0xA1,
            SSD1327_SETDISPLAYOFFSET = 0xA2,
            SSD1327_NORMALDISPLAY = 0xA4,
            SSD1327_DISPLAYALLON = 0xA5,
            SSD1327_DISPLAYALLOFF = 0xA6,
            SSD1327_INVERTDISPLAY = 0xA7,
            SSD1327_SETMULTIPLEX = 0xA8,
            SSD1327_REGULATOR = 0xAB,
            SSD1327_DISPLAYOFF = 0xAE,
            SSD1327_DISPLAYON = 0xAF,

            SSD1327_PHASELEN = 0xB1,
            SSD1327_DCLK = 0xB3,
            SSD1327_PRECHARGE2 = 0xB6,
            SSD1327_GRAYTABLE = 0xB8,
            SSD1327_PRECHARGE = 0xBC,
            SSD1327_SETVCOM = 0xBE,

            SSD1327_FUNCSELB = 0xD5,

            SSD1327_CMDLOCK = 0xFD,
        }
    }
}
