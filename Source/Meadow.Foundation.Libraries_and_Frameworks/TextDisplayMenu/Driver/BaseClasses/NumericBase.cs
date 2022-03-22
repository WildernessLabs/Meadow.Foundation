using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class NumericBase : InputBase
    {
        readonly byte scale = 0;
        int[] numberParts;
        
        int position = 0;
        int max = 0;
        int min = 0;

        public override event ValueChangedHandler ValueChanged;

        public NumericBase(int min, int max, byte scale)
        {
            this.max = max;
            this.min = min;
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

            ParseValue(currentValue);
            UpdateInputLine(NumericDisplay);
        }

        //Up
        public override bool Previous()
        {
            Console.WriteLine("Next");
            if (position == 0)
            {
                if (numberParts[position] < max) { numberParts[position]++; }
                else { numberParts[position + 1] = 0; }
            }
            else
            {
                if (numberParts[position - 1] != max && numberParts[position] < (InputHelpers.Exp(10, scale) - 1)) { numberParts[position]++; }
            }
            UpdateInputLine(NumericDisplay);

            return true;
        }

        //Down
        public override bool Next()
        {
            Console.WriteLine("Previous");
            if (position == 0)
            {
                if (numberParts[position] > min) { numberParts[position]--; }
            }
            else
            {
                if (numberParts[position] > 0) { numberParts[position]--; }
            }
            UpdateInputLine(NumericDisplay);

            return true;
        }

        public override bool Select()
        {
            if (position < numberParts.Length - 1)
            {
                position++;
            }
            else
            {
                ValueChanged(this, new ValueChangedEventArgs(itemID, scale == 0 ? numberParts[0] : double.Parse(NumericDisplay)));
            }
            return true;
        }

        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return;
            }

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