using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class NumericBase : InputBase
    {
        byte scale = 0;
        int[] numberParts;
        
        int _pos = 0;
        int _max = 0;
        int _min = 0;

        public override event ValueChangedHandler ValueChanged;

        public NumericBase(int min, int max, byte scale)
        {
            _max = max;
            _min = min;
            this.scale = scale;
        }

        string NumericDisplay
        {
            get
            {
                string value = string.Empty;
                value += numberParts[0].ToString();

                if(scale > 0)
                {
                    value += ".";
                    value += InputHelpers.PadLeft(numberParts[1].ToString(), '0', scale);
                }
                
                return InputHelpers.PadLeft(value, ' ', display.DisplayConfig.Width);
            }
        }

        public override void GetInput(string itemID, object currentValue)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Init() must be called before getting input.");
            }

            numberParts = new int[scale > 0 ? 2 : 1];
            base.itemID = itemID;
            display.ClearLines();
            display.WriteLine("Enter " + itemID, 0);
            display.SetCursorPosition(0, 1);

            ParseValue(currentValue);
            UpdateInputLine(NumericDisplay);
        }

        protected override void Next()
        {
            if (_pos == 0)
            {
                if (numberParts[_pos] < _max) { numberParts[_pos]++; }
                else { numberParts[_pos + 1] = 0; }
            }
            else
            {
                if (numberParts[_pos - 1] != _max && numberParts[_pos] < (InputHelpers.Exp(10, scale) - 1)) { numberParts[_pos]++; }
            }
            UpdateInputLine(NumericDisplay);
        }

        protected override void Previous()
        {
            if (_pos == 0)
            {
                if (numberParts[_pos] > _min) { numberParts[_pos]--; }
            }
            else
            {
                if (numberParts[_pos] > 0) { numberParts[_pos]--; }
            }
            UpdateInputLine(NumericDisplay);
        }

        protected override void Select()
        {
            if (_pos < numberParts.Length - 1)
            {
                _pos++;
            }
            else
            {
                ValueChanged(this, new ValueChangedEventArgs(itemID, scale == 0 ? numberParts[0] : double.Parse(NumericDisplay)));
            }
        }

        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            string currentValue = value.ToString();

            if (currentValue.IndexOf('.') > 0)
            {
                var parts = currentValue.Split(new char[] { '.' });
                numberParts[0] = int.Parse(parts[0]);
                numberParts[1] = int.Parse(parts[1].Substring(0, scale));
            }
            else
            {
                numberParts[0] = int.Parse(currentValue);
            }
        }
    }
}