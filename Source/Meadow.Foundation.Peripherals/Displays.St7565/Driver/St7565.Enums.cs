namespace Meadow.Foundation.Displays
{
    /// <summary>
    ///     Provide an interface to the ST7565 family of displays.
    /// </summary>
    public partial class St7565
    {
        /// <summary>
        ///     Allow the programmer to set the scroll direction.
        /// </summary>
        public enum ScrollDirection
        {
            /// <summary>
            ///     Scroll the display to the left.
            /// </summary>
            Left,
            /// <summary>
            ///     Scroll the display to the right.
            /// </summary>
            Right,
            /// <summary>
            ///     Scroll the display from the bottom left and vertically.
            /// </summary>
            RightAndVertical,
            /// <summary>
            ///     Scroll the display from the bottom right and vertically.
            /// </summary>
            LeftAndVertical
        }

        enum DisplayCommand : byte
        {
            DisplayOff = 0xAE,
            DisplayOn = 0xAF,
            DisplayStartLine = 0x40,
            PageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x00,
            AdcSelectNormal = 0xA0, // X axis flip OFF
            AdcSelectReverse = 0xA1, // X axis flip ON
            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOff = 0xA4,
            AllPixelsOn = 0xA5,
            LcdVoltageBias9 = 0xA2,
            LcdVoltageBias7 = 0xA3,
            EnterReadModifyWriteMode = 0xE0,
            ClearReadModifyWriteMode = 0xEE,
            ResetLcdModule = 0xE2,
            ShlSelectNormal = 0xC0, // Y axis flip OFF
            ShlSelectReverse = 0xC8, // Y axis flip ON
            PowerControl = 0x28,
            RegulatorResistorRatio = 0x20,
            ContrastRegister = 0x81,
            ContrastValue = 0x00,
            NoOperation = 0xE3
        }
    }
}