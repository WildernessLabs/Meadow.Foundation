namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the Sh1106 family of displays
    /// </summary>
    public partial class Sh1106
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

        enum DisplayCommand : byte
        {
            DisplayOff = 0xAE,
            DisplayOn = 0xAF,
            DisplayOnResume = 0xA4,
            DisplayStartLine = 0x40,
            PageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x02,
            SetPageAddress = 0xB0,
            ColumnAddress = 0x21,
            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOn = 0xA5,
            SetContrast = 0x81,
            SetDisplayClockDiv = 0xD5,
            SetDisplayOffset = 0xD3,
            OutputFollowsRam = 0xA4,
            MemoryMode = 0x20,

            MultiplexModeSet = 0xA8,
            MultiplexDataSet = 0x3F,

            SetChargePump = 0x8D,
            SetPrecharge = 0xD9,
            SetComPins = 0xDA,
            SetComDetect = 0xDB,
            ComScanInc = 0xC0,
            ComScanDec = 0xC8,

            SegInvOn = 0xA1,
            SegInvNormal = 0xA0,

            SetSegmentRemap = 0xA1,
        }
    }
}