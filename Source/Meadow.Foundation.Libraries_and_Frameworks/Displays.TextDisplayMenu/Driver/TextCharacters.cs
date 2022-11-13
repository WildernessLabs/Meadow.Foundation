namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    /// <summary>
    /// TextCharacters class for custom characters on LCD character displays
    /// </summary>
    public static class TextCharacters
    {
        /// <summary>
        /// Right arrow character
        /// </summary>
        public static CustomCharacter RightArrow = new CustomCharacter() { CharMap = new byte[] { 0x0, 0x8, 0xc, 0xe, 0xc, 0x8, 0x0, 0x0 }, MemorySlot = 5 };

        /// <summary>
        /// Right arrow selected character
        /// </summary>
        public static CustomCharacter RightArrowSelected = new CustomCharacter() { CharMap = new byte[] { 0x1f, 0x17, 0x13, 0x11, 0x13, 0x17, 0x1f, 0x0 }, MemorySlot = 6 };

        /// <summary>
        /// Box selected character
        /// </summary>
        public static CustomCharacter BoxSelected = new CustomCharacter() { CharMap = new byte[] { 0x0, 0x0, 0xe, 0xe, 0xe, 0x0, 0x0, 0x0 }, MemorySlot = 7 };
    }

    /// <summary>
    /// Custom character class
    /// </summary>
    public class CustomCharacter
    {
        /// <summary>
        /// Character data
        /// </summary>
        public byte[] CharMap { get; set; }

        /// <summary>
        /// Memory index for the custom character
        /// </summary>
        public byte MemorySlot { get; set; } = 0;
    }
}