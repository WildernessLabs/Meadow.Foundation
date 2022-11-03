using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Represents a base input menu item
    /// </summary>
    public abstract class InputBase : IMenuInputItem
    {
        /// <summary>
        /// The ITextDisplay object
        /// </summary>
        protected ITextDisplay display = null;

        /// <summary>
        /// Is the item initialized
        /// </summary>
        protected bool isInitialized;

        /// <summary>
        /// The item id
        /// </summary>
        protected string itemID;

        /// <summary>
        /// The event raised when the menu item value changes
        /// </summary>
        public abstract event ValueChangedHandler ValueChanged;

        public abstract void GetInput(string itemID, object currentValue);
        protected abstract void ParseValue(object value);

        public void Init(ITextDisplay display)
        {
            this.display = display;
            isInitialized = true;
        }

        protected void UpdateInputLine(string text)
        {
            display.ClearLine(1);
            display.WriteLine(text, 1, true);
            display.Show();
        }

        public abstract bool Previous();
        public abstract bool Next();
        public abstract bool Select();
    }
}