using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.Lcd
{
    /// <summary>
    /// Represents a single or multirow LCD character display
    /// </summary>
    public class CharacterDisplay : ITextDisplay
    {
        readonly ICharacterDisplay characterDisplay;

        /// <summary>
        /// The display configuration for text display menu
        /// </summary>
        public TextDisplayConfig DisplayConfig => characterDisplay?.DisplayConfig;

        /// <summary>
        /// Create a new character display object using GPIO
        /// </summary>
        /// <param name="device">The device connected to the display</param>
        /// <param name="pinRS">The RS pin</param>
        /// <param name="pinE">The E pin</param>
        /// <param name="pinD4">The D4 pin</param>
        /// <param name="pinD5">The D5 pin</param>
        /// <param name="pinD6">The D6 pin</param>
        /// <param name="pinD7">The D7 pin</param>
        /// <param name="rows">The number of character rows</param>
        /// <param name="columns">The number of character columns</param>
        public CharacterDisplay(
            IMeadowDevice device,
            IPin pinRS,
            IPin pinE,
            IPin pinD4,
            IPin pinD5,
            IPin pinD6,
            IPin pinD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(device, pinRS, pinE, pinD4, pinD5, pinD6, pinD7, rows, columns);
        }

        /// <summary>
        /// Create a new CharacterDisplay object
        /// </summary>
        /// <param name="portRS">Port for RS pin</param>
        /// <param name="portE">Port for W pin</param>
        /// <param name="portD4">Port for D4 pin</param>
        /// <param name="portD5">Port for D5 pin</param>
        /// <param name="portD6">Port for D6 pin</param>
        /// <param name="portD7">Port for D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public CharacterDisplay(
            IDigitalOutputPort portRS,
            IDigitalOutputPort portE,
            IDigitalOutputPort portD4,
            IDigitalOutputPort portD5,
            IDigitalOutputPort portD6,
            IDigitalOutputPort portD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(portRS, portE, portD4, portD5, portD6, portD7, rows, columns);
        }

        /// <summary>
        /// Create a new CharacterDisplay object
        /// </summary>
        /// <param name="device">The device connected to the display</param>
        /// <param name="pinV0">V0 pin</param>
        /// <param name="pinRS">RS pin</param>
        /// <param name="pinE">W pin</param>
        /// <param name="pinD4">D4 pin</param>
        /// <param name="pinD5">D5 pin</param>
        /// <param name="pinD6">D6 pin</param>
        /// <param name="pinD7">D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public CharacterDisplay(
            IMeadowDevice device,
            IPin pinV0,
            IPin pinRS,
            IPin pinE,
            IPin pinD4,
            IPin pinD5,
            IPin pinD6,
            IPin pinD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(device, pinV0, pinRS, pinE, pinD4, pinD5, pinD6, pinD7, rows, columns);
        }

        /// <summary>
        /// Create a new CharacterDisplay object
        /// </summary>
        /// <param name="portV0">PWM port for backlight</param>
        /// <param name="portRS">Port for RS pin</param>
        /// <param name="portE">Port for W pin</param>
        /// <param name="portD4">Port for D4 pin</param>
        /// <param name="portD5">Port for D5 pin</param>
        /// <param name="portD6">Port for D6 pin</param>
        /// <param name="portD7">Port for D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public CharacterDisplay(
            IPwmPort portV0,
            IDigitalOutputPort portRS,
            IDigitalOutputPort portE,
            IDigitalOutputPort portD4,
            IDigitalOutputPort portD5,
            IDigitalOutputPort portD6,
            IDigitalOutputPort portD7,
            byte rows = 4, byte columns = 20)
        {
            characterDisplay = new GpioCharacterDisplay(portV0, portRS, portE, portD4, portD5, portD6, portD7, rows, columns);
        }

        /// <summary>
        /// Create a new CharacterDisplay object 
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the display</param>
        /// <param name="address">The I2C address</param>
        /// <param name="rows">The number of charcter rows</param>
        /// <param name="columns">The number of character columns</param>
        /// <param name="isGroveDisplay">True if this is a Seeed Studio Grove display (default is false)</param>
        public CharacterDisplay(II2cBus i2cBus,
            byte address = (byte)I2cCharacterDisplay.Addresses.Default,
            byte rows = 4, byte columns = 20,
            bool isGroveDisplay = false)
        {
            if(isGroveDisplay)
            {
                characterDisplay = new GroveCharacterDisplay(i2cBus, address, rows, columns);
            }
            else
            {
                characterDisplay = new I2cCharacterDisplay(i2cBus, address, rows, columns);
            }
        }

        /// <summary>
        /// Clear a line of text
        /// </summary>
        /// <param name="lineNumber">The line to clear (0 indexed)</param>
        public void ClearLine(byte lineNumber)
        {
            characterDisplay?.ClearLine(lineNumber);
        }

        /// <summary>
        /// Clear all lines
        /// </summary>
        public void ClearLines()
        {
            characterDisplay?.ClearLines();
        }

        /// <summary>
        /// Save a custom character to the display
        /// </summary>
        /// <param name="characterMap">The character data</param>
        /// <param name="address">The display character address (0-7)</param>
        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            characterDisplay?.SaveCustomCharacter(characterMap, address);
        }

        /// <summary>
        /// Set the cursor position
        /// </summary>
        /// <param name="column">The cursor column</param>
        /// <param name="line">The cursor line</param>
        public void SetCursorPosition(byte column, byte line)
        {
            characterDisplay?.SetCursorPosition(column, line);
        }

        /// <summary>
        /// Write a string to the display
        /// </summary>
        /// <param name="text">The text to show as a string</param>
        public void Write(string text)
        {
            characterDisplay?.Write(text);
        }

        /// <summary>
        /// Write text to a line
        /// </summary>
        /// <param name="text">The text to dislay</param>
        /// <param name="lineNumber">The target line</param>
        /// <param name="showCursor">If true, show the cursor</param>
        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            characterDisplay?.WriteLine(text, lineNumber);
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {
            characterDisplay?.Show();
        }
    }
}