using System;

namespace Netduino.Foundation.Displays
{
    public interface ITextDisplay
    {
        TextDisplayConfig DisplayConfig { get; }
        void WriteLine(string text, byte lineNumber);

        void Write(string text);

        void SetCursorPosition(byte column, byte line);
        void Clear();

        void ClearLine(byte lineNumber);

        void SetBrightness(float brightness = 0.75f);

        /// <summary>
        ///  is this going to be supported by all text displays?
        /// </summary>
        /// <param name="characterMap"></param>
        /// <param name="address"></param>
        void SaveCustomCharacter(byte[] characterMap, byte address);
    }
}
