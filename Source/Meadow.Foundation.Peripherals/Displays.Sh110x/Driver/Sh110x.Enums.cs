using System;

namespace Meadow.Foundation.Displays
{
    public partial class Sh110x
    {
        /// <summary>
        /// Allow the programmer to set the scroll direction
        /// </summary>
        public enum ScrollDirection
        {
            /// <summary>
            /// Scroll the display to the left
            /// </summary>
            Left,
            /// <summary>
            /// Scroll the display to the right
            /// </summary>
            Right,
            /// <summary>
            /// Scroll the display from the bottom left and vertically
            /// </summary>
            RightAndVertical,
            /// <summary>
            /// Scroll the display from the bottom right and vertically
            /// </summary>
            LeftAndVertical
        }

        [Flags]
        internal enum DisplayCommand : byte
        {
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Turns the Display off (Power Save Mode)
            /// </summary>
            DisplayOff = 0xAE,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Turns the Display On
            /// </summary>
            DisplayOn = 0xAF,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Restores normal operation after <see cref="DisplayAllPixelsOn"/> has turned all pixels on.
            /// </summary>
            DisplayResume = 0xA4,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Turns on all pixels on the display. Ignores <see cref="DisplayVideoReverse"/>. Restore normal operation wth <see cref="DisplayResume"/>
            /// </summary>
            DisplayAllPixelsOn = 0xA5,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the display to "Normal" mode. A logical 1 in memory will turn on the OLED pixel.
            /// </summary>
            DisplayVideoNormal = 0xA6,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the display to "Reverse" mode. A logical 1 in memory will turn off the OLED pixel.
            /// </summary>
            DisplayVideoReverse = 0xA7,
            /// <summary>
            /// <para>(SH1106)</para>
            /// Modify this command by adding a value from 0x0 to 0x7 to select the page address to use.
            /// <para>(SH1107)</para>
            /// Modify this command by adding a value from 0x0 to 0xF to select the page address to use.
            /// </summary>
            PageAddress = 0xB0,
            /// <summary>
            /// <para>(SH1106 only. For SH1107, see <see cref="SetDisplayStartLine"/>)</para>
            /// Modify this command by adding a value from 0x00 to 0x3F to set the address of the initial display line. 
            /// The RAM display data at this line becomes the top line of OLED screen.
            /// Changing the Start Line while displaying effectively scrolls the display or acts as a page change..
            /// </summary>
            DisplayStartLine = 0x40,
            /// <summary>
            /// <para>(SH1107 only. For SH1106, see <see cref="DisplayStartLine"/>)</para>
            /// The next byte sent sets the address of the initial display line. 
            /// The RAM display data at this line becomes the top line of OLED screen.
            /// Changing the line address while displaying effectively scrolls the display or acts as a page change.
            /// Default value after reset is 0x00.
            /// </summary>
            SetDisplayStartLine = 0xDC,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Modify this command by adding a value from 0x0 to 0xF to set the lower 4 bits of the column address.
            /// </summary>
            ColumnAddressLow = 0x00,
            /// <summary>
            /// <para>(SH1106)</para>
            /// Modify this command by adding a value from 0x0 to 0x8 to set the upper 4 bits of the column address.
            /// <para>(SH1107)</para>
            /// Modify this command by adding a value from 0x0 to 0x7 to set the upper 3 bits of the column address.
            /// </summary>
            ColumnAddressHigh = 0x10,
            /// <summary>
            /// <para>(SH1107 only)</para>
            /// Sets the display memory to auto-increment to the next column of the current page after each byte of display data, wrapping around after 127.
            /// If <see cref="SetSegmentInverted"/> is applied, the direction of the auto-increment is reversed.
            /// </summary>
            PageAddressMode = 0x20,
            /// <summary>
            /// <para>(SH1107 only)</para>
            /// Sets the display memory to auto-increment to the next page of the current column after each byte of display data, wrapping around after 127.
            /// If <see cref="SetSegmentInverted"/> is applied, the direction of the auto-increment is reversed.
            /// </summary>
            VerticalAddressMode = 0x21,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the "Common" Scan direction to the default direction.
            /// Opposite of <seealso cref="ScanDirectionInverted"/>
            /// </summary>
            ScanDirectionStandard = 0xC0,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the "Common" Scan direction to the inverted direction, effectively flipping the display horizontally (column order).
            /// Opposite of <seealso cref="ScanDirectionStandard"/>
            /// </summary>
            ScanDirectionInverted = 0xC8,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the display to the default auto-increment direction and order of display data bits.
            /// Opposite of <seealso cref="SetSegmentInverted"/>
            /// </summary>
            SetSegmentNormal = 0xA0,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Sets the display to automatically reverse the auto-increment direction and order of display data bits, effectively flipping the display vertically (page order).
            /// Opposite of <seealso cref="SetSegmentNormal"/>
            /// </summary>
            SetSegmentInverted = 0xA1,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// The next byte sent sets the contrast setting of the display. The chip has 256 contrast steps from 0x00 to 0xFF. 
            /// Default value after reset is 0x80.
            /// </summary>
            SetContrast = 0x81,
            /// <summary>
            /// <para>(SH1106)</para>
            /// The next byte sent sets the column mapping of the first display line from 0x00 to 0x3F. 
            /// Default value after reset is 0x00.
            /// <para>(SH1107)</para>
            /// The next byte sent sets the column mapping of the first display line from 0x00 to 0x7F. 
            /// Default value after reset is 0x00.
            /// </summary>
            SetDisplayOffset = 0xD3,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// A NOP (No Operation)
            /// </summary>
            NOP = 0xE3,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// The next byte sent sets the frequency of the internal display clocks (DCLKs). 
            /// The lower 4 bits define as the divide ratio (Value from 1 to 16, represented using 0x_0 to 0x_F) used to divide the oscillator frequency.
            /// The upper 4 bits adjust the base oscillator frequency by -25% (0x0_) to +50% (0xF_) in 5% steps. (0x5_ is 0%) 
            /// Default value after reset is 0x50
            /// </summary>
            SetDisplayClockDiv = 0xD5,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// The next byte sent sets the Pre-charge and Discharge period for the display in clocks.
            /// The lower 4 bits are used for pre-charge. (Default 0x_2)
            /// The upper 4 bits are used for discharge. (Default 0x2_)
            /// Default value after reset is 0x22.
            /// </summary>
            SetChargePeriods = 0xD9,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// This command is to control the DC-DC voltage converter status (and the switching frequency). 
            /// Issuing this command, followed by <see cref="DCDCOn"/>, followed by <see cref="DisplayOn"/> will turn on the converter. 
            /// The panel display must be off while issuing this command.
            /// </summary>
            SetDCDCStatus = 0xAD,
            /// <summary>
            /// <para>(SH1107 only)</para>
            /// Value for <see cref="SetDCDCStatus"/> command. (Also sets Charge pump switching frequency using bits 1-3)
            /// </summary>
            DCDCOffFrequencyAdjust = 0x80,
            /// <summary>
            /// <para>(SH1107 only)</para>
            /// Value for <see cref="SetDCDCStatus"/> command. (Also sets Charge pump switching frequency using bits 1-3)
            /// </summary>
            DCDCOnFrequencyAdjust = 0x81,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Value for <see cref="SetDCDCStatus"/> command.
            /// </summary>
            DCDCOff = 0x8A,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// Value for <see cref="SetDCDCStatus"/> command.
            /// </summary>
            DCDCOn = 0x8B,
            /// <summary>
            /// <para>(SH1106 only)</para>
            /// Modify this command by adding a value from 0x0 to 0x3 to set the charge pump voltage.
            /// Default is 0x32.
            /// </summary>
            PumpVoltage = 0x30,
            /// <summary>
            /// <para>(SH1106)</para>
            /// The next byte sent sets the multiplex ratio from 1 to 64 (Represented as 0x00 to 0x3F).
            /// Default value after reset is 0x3F.
            /// <para>(SH1107)</para>
            /// The next byte sent sets the multiplex ratio from 1 to 128 (Represented as 0x00 to 0x7F).
            /// Default value after reset is 0x7F.
            /// </summary>
            SetMultiplexRatio = 0xA8,
            /// <summary>
            /// <para>(SH1106 and SH1107)</para>
            /// The next byte sent sets the output voltage for the currently de-selected Common pins. (See datasheet)
            /// Default value after reset is 0x35.
            /// </summary>
            SetVComDeselect = 0xDB,
            /// <summary>
            /// <para>(SH1106 only)</para>
            /// The next byte sets the common signals pad configuration (sequential or alternative) to match the OLED panel hardware layout.
            /// Valid Values are <see cref="ComPinsSequential"/> (0x02) and <see cref="ComPinsAlternative"/> (0x12).
            /// Default value after reset is <see cref="ComPinsAlternative"/> (0x12).
            /// </summary>
            SetComPins = 0xDA,
            /// <summary>
            /// <para>(SH1106 only)</para> Value for <see cref="SetComPins"/>
            /// </summary>
            ComPinsSequential = 0x02,
            /// <summary>
            /// <para>(SH1106 only)</para> Value for <see cref="SetComPins"/>
            /// </summary>
            ComPinsAlternative = 0x12,
        }

        /// <summary>
        /// Valid I2C addresses for the display
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x3C
            /// Commonly used with 128x32 displays
            /// </summary>
            Address_0x3C = 0x3C,
            /// <summary>
            /// Bus address 0x3D
            /// </summary>
            Address_0x3D = 0x3D,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x3D
        }

        /// <summary>
        /// The display connection type
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// SPI
            /// </summary>
            SPI,
            /// <summary>
            /// I2C
            /// </summary>
            I2C,
        }
    }
}