namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Provide an interface to the Ch1115 family of displays
    /// </summary>
    public partial class Ch1115
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
            DisplayStartLine = 0x40,
            PageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x00,
            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOff = 0xA4,
            AllPixelsOn = 0xA5,
            ContrastValue = 0x00,
            IRefRestigerSet = 0x82,
            IRefRestigerAdjust = 0x00,
            SetPumpReg = 0x30,
            SetPumpSet = 0x01,
            SegSetRemap = 0xA0,
            SegSetPads = 0xA2,

            MultiplexModeSet = 0xA8,
            MultiplexDataSet = 0x3F,

            CommonScanDir = 0xC0,

            OffsetModeSet = 0xD3,
            OffsetDataSet = 0x00,

            PrechargeModeSet = 0xD9,
            PrechargeDataSet = 0x22,

            LevelModeSet = 0xDB,
            LevelDataSet = 0x40,

            OscFrequencyModeSet = 0xD5,
            OscFrequencyDataSet = 0xA0,

            ComLevelModeSet = 0xDB,
            ComLevelDataSet = 0x40,

            DCModeSet = 0xAD,
            DCOnOffSet = 0x8B,
        }
    }
}