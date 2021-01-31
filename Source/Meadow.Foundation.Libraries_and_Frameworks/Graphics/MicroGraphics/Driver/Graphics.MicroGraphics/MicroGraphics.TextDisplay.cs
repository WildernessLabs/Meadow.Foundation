using System;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Graphics
{
    public partial class GraphicsLibrary : ITextDisplay
    {
        public void Write(string text)
        {
            if (CurrentFont == null)
            {
                throw new Exception("GraphicsLibrary.Write requires CurrentFont to be set");
            }
            DrawText(CurrentFont.Width * CursorColumn, CurrentFont.Height * CursorLine, text, Color.White);
        }

        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            if (CurrentFont == null)
            {
                throw new Exception("GraphicsLibrary.WriteLine requires CurrentFont to be set");
            }
            DrawText(0, lineNumber * CurrentFont.Height, text, Color.White);

            if (CursorLine == lineNumber && showCursor == true)
            {
                DrawCursor();
            }
        }

        void DrawCursor()
        {
            InvertRectangle(CursorColumn * currentFont.Width, CursorLine * currentFont.Height,
                currentFont.Width, currentFont.Height);
        }

        public void ClearLines()
        {
            Clear(false); //for now
        }

        public void ClearLine(byte lineNumber)
        {
            DrawRectangle(0, CurrentFont.Height * lineNumber, (int)Width, CurrentFont.Height, false, true);
        }

        public byte CursorColumn { get; private set; } = 0;
        public byte CursorLine { get; private set; } = 0;
        public void SetCursorPosition(byte column, byte line)
        {
            CursorColumn = column;
            CursorLine = line;
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            //  throw new NotImplementedException();
        }
    }
}