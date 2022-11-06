using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    /// <summary>
    /// Text display Menu Input abstraction
    /// </summary>
    public interface IMenuInputItem : IPage
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="display">The display to render the menu</param>
        void Init(ITextDisplay display);

        /// <summary>
        /// Get input
        /// </summary>
        /// <param name="itemID">Item id</param>
        /// <param name="currentValue">Current value</param>
        void GetInput(string itemID, object currentValue);

        /// <summary>
        /// Raised when the value changes
        /// </summary>
        event ValueChangedHandler ValueChanged;
    }
}