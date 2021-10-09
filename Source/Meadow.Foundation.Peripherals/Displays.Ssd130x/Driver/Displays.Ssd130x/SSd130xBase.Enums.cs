namespace Meadow.Foundation.Displays.Ssd130x
{
    /// <summary>
    /// Provide an interface to the SSD1306 family of OLED displays.
    /// </summary>
    public abstract partial class Ssd130xBase
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x3C,
            Address1 = 0x3D,
            Default = Address0
        }

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

        /// <summary>
        ///     Supported display types.
        /// </summary>
        public enum DisplayType
        {
            /// <summary>
            ///     0.96 128x64 pixel display.
            /// </summary>
            OLED128x64,
            /// <summary>
            ///     0.91 128x32 pixel display.
            /// </summary>
            OLED128x32,
            /// <summary>
            ///     64x48 pixel display.
            /// </summary>
            OLED64x48,
            /// <summary>
            ///     96x16 pixel display.
            /// </summary>
            OLED96x16,
            /// <summary>
            ///     70x40 pixel display.
            /// </summary>
            OLED72x40,
        }

        public enum ConnectionType
        {
            SPI,
            I2C,
        }
    }
}
