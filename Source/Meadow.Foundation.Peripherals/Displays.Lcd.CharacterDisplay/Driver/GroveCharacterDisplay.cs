using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Lcd
{
    /// <summary>
    /// Represents a Grove chracter display
    /// </summary>
    public class GroveCharacterDisplay : I2cCharacterDisplay
    {
        /// <summary>
        /// Create a new Grove chracter display object
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the display</param>
        /// <param name="address">The I2C address</param>
        /// <param name="rows">The number of character rows</param>
        /// <param name="columns">The number of character columns</param>
        public GroveCharacterDisplay(II2cBus i2cBus,
            byte address = (byte)Addresses.Address_0x3E,
            byte rows = 2, byte columns = 16)
            : base(i2cBus, address, rows, columns)
        {
        }

        /// <summary>
        /// Initialize the display
        /// </summary>
        protected override void Initialize()
        {
            var displayFunction = (byte)(LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS);

            if (DisplayConfig.Height > 1)
            {
                displayFunction |= LCD_2LINE;
            }

            Thread.Sleep(50);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));
            Thread.Sleep(50);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));
            Thread.Sleep(2);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));

            // turn the display on with no cursor or blinking default
            displayControl = (byte)(LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF);
            DisplayOn();

            // clear it off
            ClearLines();

            // Initialize to default text direction (for romance languages)
            displayMode = (byte)(LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT);
            // set the entry mode
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="value">The command as a byte</param>
        protected override void Command(byte value)
        {
            var data = new byte[] { 0x80, value };
            i2cComms.Write(data);
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="value">The data to send</param>
        /// <param name="mode">The mode (not used)</param>
        protected override void Send(byte value, byte mode)
        {
            var data = new byte[] { 0x40, value };
            i2cComms.Write(data);
        }
    }
}