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

        internal enum DisplayCommand : byte
        {
            DCDC = 0xAD,
            DisplayOff = 0xAE,
            DisplayOn = 0xAF,
            DisplayOnResume = 0xA4,
            DisplayStartLine = 0x40,
            SetPageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x02,

            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOn = 0xA5,
            SetContrast = 0x81,
            SetDisplayClockDiv = 0xD5,
            SetDisplayOffset = 0xD3,
            OutputFollowsRam = 0xA4,
            MemoryMode = 0x20,
            ColumnAddress = 0x21,
            PageAddress = 0x22,

            MultiplexModeSet = 0xA8,
            MultiplexDataSet = 0x3F,

            SetChargePump = 0x8D,
            SetPrecharge = 0xD9,
            SetComPins = 0xDA,
            SetVComDetect = 0xDB,
            ComScanInc = 0xC0,
            ComScanDec = 0xC8,

            SegInvOn = 0xA1,
            SegInvNormal = 0xA0,

            SetSegmentRemap = 0xA1,
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