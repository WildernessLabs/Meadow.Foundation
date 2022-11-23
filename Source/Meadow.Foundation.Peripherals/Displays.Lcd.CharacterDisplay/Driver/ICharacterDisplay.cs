using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.Lcd
{
    /// <summary>
    /// Character display abstraction
    /// </summary>
    public interface ICharacterDisplay : ITextDisplay
    {
        /// <summary>
        /// Save a custom character to the display
        /// </summary>
        /// <param name="characterMap">The character data</param>
        /// <param name="address">The display character address (0-7)</param>
        public void SaveCustomCharacter(byte[] characterMap, byte address);
    }
}