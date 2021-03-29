using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.Lcd
{
    public class CharacterDisplay : ITextDisplay
    {
        protected ICharacterDisplay characterDisplay;

        public TextDisplayConfig DisplayConfig => characterDisplay?.DisplayConfig;

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

        public CharacterDisplay(II2cBus i2cBus, byte address = I2cCharacterDisplay.DefaultI2cAddress, byte rows = 4, byte columns = 20)
        {
            characterDisplay = new I2cCharacterDisplay(i2cBus, address, rows, columns);
        }

        public void ClearLine(byte lineNumber)
        {
            characterDisplay?.ClearLine(lineNumber);
        }

        public void ClearLines()
        {
            characterDisplay?.ClearLines();
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            characterDisplay?.SaveCustomCharacter(characterMap, address);
        }

        public void SetCursorPosition(byte column, byte line)
        {
            characterDisplay?.SetCursorPosition(column, line);
        }

        public void Write(string text)
        {
            characterDisplay?.Write(text);
        }

        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            characterDisplay?.WriteLine(text, lineNumber);
        }

        public void Show()
        {
            characterDisplay?.Show();
        }
    }
}