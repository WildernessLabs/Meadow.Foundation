using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class InputBase : IMenuInputItem
    {
        protected ITextDisplay display = null;
        protected bool isInitialized;
        protected string itemID;

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