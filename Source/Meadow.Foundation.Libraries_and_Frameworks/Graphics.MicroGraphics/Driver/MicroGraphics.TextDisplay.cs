using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics : ITextDisplay
    {
        /// <summary>
        /// Write text to the display
        /// </summary>
        /// <param name="text">The text</param>
        /// <exception cref="Exception">Throws if no font is set</exception>
        public void Write(string text)
        {
            if (CurrentFont == null)
            {
                throw new Exception("MicroGraphics.Write requires CurrentFont to be set");
            }

            DrawText(CurrentFont.Width * CursorColumn * DisplayConfig.FontScale,
                CurrentFont.Height * CursorLine * DisplayConfig.FontScale,
                text,
                Color.White);
        }

        /// <summary>
        /// Write a line of text in White
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="lineNumber">The line to write</param>
        /// <param name="showCursor">True to show the cursor</param>
        /// <exception cref="Exception">Throws if no font is set</exception>
        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            WriteLine(text, lineNumber, Color.White, showCursor);
        }

        /// <summary>
        /// Write a line of text
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="lineNumber">The line to write</param>
        /// <param name="showCursor">True to show the cursor</param>
        /// <param name="textColor">Optional color to use for drawing the text</param>
        /// <exception cref="Exception">Throws if no font is set</exception>
        public void WriteLine(string text, byte lineNumber, Color textColor, bool showCursor = false)
        {
            if (CurrentFont == null)
            {
                throw new Exception("MicroGraphics.WriteLine requires CurrentFont to be set");
            }
            DrawText(0, lineNumber * CurrentFont.Height * DisplayConfig.FontScale,
                text, textColor, (ScaleFactor)DisplayConfig.FontScale);

            if (CursorLine == lineNumber && showCursor == true)
            {
                DrawCursor();
            }
        }

        private void DrawCursor()
        {
            if (currentFont != null)
            {
                InvertRectangle(CursorColumn * currentFont.Width * DisplayConfig.FontScale,
                    CursorLine * currentFont.Height * DisplayConfig.FontScale,
                    currentFont.Width, currentFont.Height);
            }
        }

        /// <summary>
        /// Clear all lines of text
        /// </summary>
        public void ClearLines()
        {
            Clear(false);
        }

        /// <summary>
        /// Clear a single line of text
        /// </summary>
        /// <param name="lineNumber">The line to clear</param>
        public void ClearLine(byte lineNumber)
        {
            DrawRectangle(0, CurrentFont.Height * lineNumber * DisplayConfig.FontScale,
                Width,
                CurrentFont.Height * DisplayConfig.FontScale,
                false, true);
        }

        /// <summary>
        /// The current cursor column relative to text/font
        /// </summary>
        public byte CursorColumn { get; private set; } = 0;

        /// <summary>
        /// The current cursor line relative to the text
        /// </summary>
        public byte CursorLine { get; private set; } = 0;

        /// <summary>
        /// Set the cursor position relative to the text
        /// </summary>
        /// <param name="column">The text column</param>
        /// <param name="line">The line column</param>
        public void SetCursorPosition(byte column, byte line)
        {
            CursorColumn = column;
            CursorLine = line;
        }
    }
}