using System;
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
            display.Write(text);
            display.SetCursorPosition(0, 1);
        }

        protected abstract void Previous();
        protected abstract void Next();
        protected abstract void Select();
    }
}