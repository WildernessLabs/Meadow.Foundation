using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.Lcd
{
    public class I2cCharacterDisplay : ICharacterDisplay
    {
        public TextDisplayConfig DisplayConfig => throw new System.NotImplementedException();

        public void ClearLine(byte lineNumber)
        {
            throw new System.NotImplementedException();
        }

        public void ClearLines()
        {
            throw new System.NotImplementedException();
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            throw new System.NotImplementedException();
        }

        public void SetCursorPosition(byte column, byte line)
        {
            throw new System.NotImplementedException();
        }

        public void Write(string text)
        {
            throw new System.NotImplementedException();
        }

        public void WriteLine(string text, byte lineNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}
