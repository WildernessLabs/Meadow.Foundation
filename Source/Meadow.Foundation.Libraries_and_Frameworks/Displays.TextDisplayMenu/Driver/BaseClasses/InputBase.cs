using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.UI.InputTypes
{
    /// <summary>
    /// Represents a base input menu item
    /// </summary>
    public abstract class InputBase : IMenuInputItem
    {
        /// <summary>
        /// The ITextDisplay object
        /// </summary>
        protected ITextDisplay display;

        /// <summary>
        /// Is the item initialized
        /// </summary>
        protected bool isInitialized;

        /// <summary>
        /// The item id
        /// </summary>
        protected string itemID = string.Empty;

        /// <summary>
        /// The event raised when the menu item value changes
        /// </summary>
        public abstract event ValueChangedHandler ValueChanged;

        /// <summary>
        /// Get input
        /// </summary>
        /// <param name="itemID">Item id</param>
        /// <param name="currentValue">Current value</param>
        public abstract void GetInput(string itemID, object currentValue);

        /// <summary>
        /// Parse value
        /// </summary>
        /// <param name="value">Value to parse</param>
        protected abstract void ParseValue(object value);

        /// <summary>
        /// Initialize the input
        /// </summary>
        /// <param name="display">The display to show the input item</param>
        public void Init(ITextDisplay display)
        {
            this.display = display;
            isInitialized = true;
            display.SetCursorPosition(0, byte.MaxValue);
        }

        /// ToDo: this should be an event and moved out of TextDisplayMenu
        /// <summary>
        /// Update the input line on the display
        /// </summary>
        /// <param name="text">The new text to display</param>
        protected void UpdateInputLine(string text)
        {
            display.ClearLine(1);
            display.WriteLine(text, 1, true);
            display.Show();
        }

        /// <summary>
        /// Previous input
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool Previous();

        /// <summary>
        /// Next input
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool Next();

        /// <summary>
        /// Select input
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool Select();

        /// <summary>
        /// Back input
        /// </summary>
        /// <returns>True if successful</returns>
        public abstract bool Back();
    }
}