using System;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class InputBase : IMenuInputItem
    {
        protected IRotaryEncoder _encoder;
        protected IButton _buttonNext;
        protected IButton _buttonPrevious;
        protected IButton _buttonSelect;
        protected ITextDisplay _display = null;
        protected bool _isInit;
        protected string _itemID;

        public abstract event ValueChangedHandler ValueChanged;

        public abstract void GetInput(string itemID, object currentValue);
        protected abstract void ParseValue(object value);

        public void Init(ITextDisplay display, IRotaryEncoder encoder, IButton buttonSelect)
        {
            _display = display;
            _encoder = encoder;
            _buttonSelect = buttonSelect;
            _isInit = true;
        }

        public void Init(ITextDisplay display, IButton buttonNext, IButton buttonPrevious, IButton buttonSelect)
        {
            _display = display;
            _buttonSelect = buttonSelect;
            _buttonNext = buttonNext;
            _buttonPrevious = buttonPrevious;
            _isInit = true;
        }

        protected void RewriteInputLine(string text)
        {
            _display.Write(text);
            _display.SetCursorPosition(0, 1);
        }

        protected void RegisterHandlers()
        {
            if (_encoder != null)
            {
                _encoder.Rotated += HandleRotated;
            }
            else
            {
                _buttonNext.Clicked += HandleNext;
                _buttonPrevious.Clicked += HandlePrevious;
            }
            _buttonSelect.Clicked += HandleClicked;
        }

        protected void UnregisterHandlers()
        {
            _buttonSelect.Clicked -= HandleClicked;
            if (_encoder != null)
            {
                _encoder.Rotated -= HandleRotated;
            }
            else
            {
                _buttonNext.Clicked -= HandleNext;
                _buttonPrevious.Clicked -= HandlePrevious;
            }
        }

        protected abstract void HandlePrevious(object sender, EventArgs e);
        protected abstract void HandleNext(object sender, EventArgs e);
        protected abstract void HandleRotated(object sender, RotaryTurnedEventArgs e);
        protected abstract void HandleClicked(object sender, EventArgs e);
    }
}
