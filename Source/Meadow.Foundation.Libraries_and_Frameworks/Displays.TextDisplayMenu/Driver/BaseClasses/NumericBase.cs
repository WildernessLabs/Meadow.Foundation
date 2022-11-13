using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Represents a base Numeric input type
    /// </summary>
    public abstract class NumericBase : InputBase
    {
        readonly byte scale = 0;
        int[] numberParts;
        
        int position = 0;
        readonly int max = 0;
        readonly int min = 0;

        /// <summary>
        /// Raised when the numeric value changes
        /// </summary>
        public override event ValueChangedHandler ValueChanged;

        /// <summary>
        /// Create a new NumericBase object
        /// </summary>
        /// <param name="min">The minimum int value</param>
        /// <param name="max">The maximum int value</param>
        /// <param name="scale">The scale or step size between values</param>
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

        /// <summary>
        /// Get the input
        /// </summary>
        /// <param name="itemID">The item id</param>
        /// <param name="currentValue">The current value</param>
        /// <exception cref="InvalidOperationException">Throws if not initialized</exception>
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

        /// <summary>
        /// Send a Previous input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Previous()
        {
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

        /// <summary>
        /// Send a Next input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Next()
        {
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

        /// <summary>
        /// Send a Select input to the item
        /// </summary>
        /// <returns>true</returns>
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

        /// <summary>
        /// Parse a value for the item
        /// </summary>
        /// <param name="value">The string value as an object</param>
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