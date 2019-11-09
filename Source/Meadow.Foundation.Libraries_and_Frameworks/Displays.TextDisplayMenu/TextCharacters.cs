using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public static class TextCharacters
    {
        public static CustomCharacter RightArrow = new CustomCharacter() { CharMap = new byte[] { 0x0, 0x8, 0xc, 0xe, 0xc, 0x8, 0x0, 0x0 }, MemorySlot = 5 };
        public static CustomCharacter RightArrowSelected = new CustomCharacter() { CharMap = new byte[] { 0x1f, 0x17, 0x13, 0x11, 0x13, 0x17, 0x1f, 0x0 }, MemorySlot = 6 };
        public static CustomCharacter BoxSelected = new CustomCharacter() { CharMap = new byte[] { 0x0, 0x0, 0xe, 0xe, 0xe, 0x0, 0x0, 0x0 }, MemorySlot = 7 };
    }

    public class CustomCharacter
    {
        public byte[] CharMap { get; set; }
        public byte MemorySlot { get; set; } = 0;

        public char ToChar()
        {
            return (char)MemorySlot;
        }

        public override string ToString()
        {
            return new string(new char[] { ToChar() });
        }
    }
}
