using System;

using Meadow.Foundation.Sensors.Rotary;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class ListBase : InputBase
    {
        protected string[] _choices;
        protected int _selectedIndex = 0;
        private int _rotatedIndex = 0;


        public override event ValueChangedHandler ValueChanged;

        string OutputDisplay
        {
            get
            {
                return InputHelpers.PadLeft(_choices[_selectedIndex], ' ', _display.DisplayConfig.Width);
            }
        }

        public override void GetInput(string itemID, object currentValue)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException("Init() must be called before getting input.");
            }

            _display.Clear();
            _display.WriteLine("Select", 0);
            _display.SetCursorPosition(0, 1);

            RegisterHandlers();
            ParseValue(currentValue);
            RewriteInputLine(OutputDisplay);
        }

        protected override void HandlePrevious(object sender, EventArgs e)
        {
            DoPrevious();
        }

        protected override void HandleNext(object sender, EventArgs e)
        {
            DoNext();
        }

        protected override void HandleClicked(object sender, EventArgs e)
        {
            UnregisterHandlers();
            ValueChanged(this, new ValueChangedEventArgs(_itemID, _choices[_selectedIndex]));
        }

        protected override void HandleRotated(object sender, RotaryTurnedEventArgs e)
        {
            if (e.Direction == RotationDirection.Clockwise)
            {
                DoNext();
            }
            else
            {
                DoPrevious();
            }
        }

        private void DoNext()
        {
            _rotatedIndex++;
            _selectedIndex = _rotatedIndex % _choices.Length;
            if (_selectedIndex < 0) _selectedIndex *= -1;
            RewriteInputLine(OutputDisplay);
        }
       
        private void DoPrevious()
        {
            _rotatedIndex--;
            _selectedIndex = _rotatedIndex % _choices.Length;
            if (_selectedIndex < 0) _selectedIndex *= -1;
            RewriteInputLine(OutputDisplay);
        }

        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            for (int i=0;i< _choices.Length; i++)
            {
                if(_choices[i] == value.ToString())
                {
                    _selectedIndex = i;
                    break;
                }
            }
        }
    }
}
