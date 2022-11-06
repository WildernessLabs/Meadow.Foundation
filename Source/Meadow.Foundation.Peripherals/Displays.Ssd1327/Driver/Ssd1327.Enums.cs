namespace Meadow.Foundation.Displays
{
    public partial class Ssd1327
    {
        /// <summary>
        /// SSD1327 commands
        /// </summary>
        protected enum Command : byte
        {
            /// <summary>
            /// Black
            /// </summary>
            BLACK = 0x0,
            /// <summary>
            /// White
            /// </summary>
            WHITE = 0xF,
            /// <summary>
            /// Set brightness
            /// </summary>
            SETBRIGHTNESS = 0x82,
            /// <summary>
            /// Set column
            /// </summary>
            SETCOLUMN = 0x15,
            /// <summary>
            /// Set row
            /// </summary>
            SETROW = 0x75,
            /// <summary>
            /// Set contrast
            /// </summary>
            SETCONTRAST = 0x81,
            /// <summary>
            /// Set lookup table
            /// </summary>
            SETLUT = 0x91,
            /// <summary>
            /// Set reg map
            /// </summary>
            SEGREMAP = 0xA0,
            /// <summary>
            /// Set start line
            /// </summary>
            SETSTARTLINE = 0xA1,
            /// <summary>
            /// Set display offset
            /// </summary>
            SETDISPLAYOFFSET = 0xA2,
            /// <summary>
            /// Normal display
            /// </summary>
            NORMALDISPLAY = 0xA4,
            /// <summary>
            /// Display all om
            /// </summary>
            DISPLAYALLON = 0xA5,
            /// <summary>
            /// Display all off
            /// </summary>
            DISPLAYALLOFF = 0xA6,
            /// <summary>
            /// Invert display
            /// </summary>
            INVERTDISPLAY = 0xA7,
            /// <summary>
            /// Set multiplex
            /// </summary>
            SETMULTIPLEX = 0xA8,
            /// <summary>
            /// Regulator
            /// </summary>
            REGULATOR = 0xAB,
            /// <summary>
            /// Display off
            /// </summary>
            DISPLAYOFF = 0xAE,
            /// <summary>
            /// Display on
            /// </summary>
            DISPLAYON = 0xAF,
            /// <summary>
            /// Phase length
            /// </summary>
            PHASELEN = 0xB1,
            /// <summary>
            /// Display clock
            /// </summary>
            DCLK = 0xB3,
            /// <summary>
            /// Precharge 2
            /// </summary>
            PRECHARGE2 = 0xB6,
            /// <summary>
            /// Gray table
            /// </summary>
            GRAYTABLE = 0xB8,
            /// <summary>
            /// Precharge 1
            /// </summary>
            PRECHARGE = 0xBC,
            /// <summary>
            /// Set VCOM
            /// </summary>
            SETVCOM = 0xBE,
            /// <summary>
            /// FUNCSELB
            /// </summary>
            FUNCSELB = 0xD5,
            /// <summary>
            /// Command lock
            /// </summary>
            CMDLOCK = 0xFD,
        }
    }
}