using System;

namespace Meadow.Foundation.Displays.UI.InputTypes
{
    /// <summary>
    /// Text display menu Date input type
    /// </summary>
    public class Date : InputBase
    {
        int[] dateParts;
        byte position = 0;

        /// <summary>
        /// Raised when the date value changes
        /// </summary>
        public override event ValueChangedHandler ValueChanged = delegate { };

        /// <summary>
        /// Create a new Date input object
        /// </summary>
        public Date()
        { }

        string DateDisplay
        {
            get
            {
                string value = string.Empty;
                for (int i = 0; i < dateParts.Length; i++)
                {
                    if (i > 0) { value += "-"; }
                    value += InputHelpers.PadLeft(dateParts[i].ToString(), '0', 2);
                }
                return InputHelpers.PadLeft(value, ' ', display.DisplayConfig.Width);
            }
        }

        byte CursorPosition
        {
            get
            {
                if (display == null)
                {
                    return 0;
                }

                return position switch
                {
                    0 => (byte)(display.DisplayConfig.Width - 7),
                    1 => (byte)(display.DisplayConfig.Width - 4),
                    _ => (byte)(display.DisplayConfig.Width - 1),
                };
            }
        }

        string DateModeDisplay => "yyyy-mm-dd";

        /// <summary>
        /// Get input from user
        /// </summary>
        /// <param name="itemID">id of the menu item</param>
        /// <param name="currentValue">current value of the menu item</param>
        public override void GetInput(string itemID, object currentValue)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Init() must be called before getting input.");
            }

            dateParts = new int[3];
            base.itemID = itemID;
            display.ClearLines();
            display.WriteLine("Enter " + DateModeDisplay, 0);

            ParseValue(currentValue);

            display.SetCursorPosition(CursorPosition, 1);
            UpdateInputLine(DateDisplay);
        }

        /// <summary>
        /// Send a Previous input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Previous()
        {
            int max;

            if (position == 0)
            {
                max = 9999;
            }
            else if (position == 1)
            {
                max = 12;
            }
            else
            {
                max = 31;
            }

            if (dateParts[position] < max) { dateParts[position]++; }
            display.SetCursorPosition(CursorPosition, 1);
            UpdateInputLine(DateDisplay);

            return true;
        }

        /// <summary>
        /// Send a Next input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Next()
        {
            int min = 0;
            if (dateParts[position] > min) { dateParts[position]--; }
            display.SetCursorPosition(CursorPosition, 1);
            UpdateInputLine(DateDisplay);

            return true;
        }

        /// <summary>
        /// Send a Select input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Select()
        {
            if (position < dateParts.Length - 1)
            {
                position++;
                display.SetCursorPosition(CursorPosition, 1);
                UpdateInputLine(DateDisplay);
            }
            else
            {
                DateTime date = new DateTime(dateParts[0], dateParts[1], dateParts[2]);
                ValueChanged(this, new ValueChangedEventArgs(itemID, date));
            }

            return true;
        }

        /// <summary>
        /// Send a Back input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Back()
        {
            if (position > 0)
            {
                position--;
                display.SetCursorPosition(CursorPosition, 1);
                UpdateInputLine(DateDisplay);
            }
            else
            {
                DateTime date = new DateTime(dateParts[0], dateParts[1], dateParts[2]);
                ValueChanged(this, new ValueChangedEventArgs(itemID, date));
            }
            return true;
        }

        /// <summary>
        /// Parse the current value
        /// </summary>
        /// <param name="value">The value to parse as a string</param>
        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            if (value is DateTime)
            {
                DateTime date = (DateTime)value;

                dateParts[0] = date.Year;
                dateParts[1] = date.Month;
                dateParts[2] = date.Day;
            }
            else
            {
                string currentValue = value.ToString();

                var parts = currentValue.Split(new char[] { '-' });

                for (int i = 0; i < parts.Length; i++)
                {
                    dateParts[i] = int.Parse(parts[i]);
                }
            }
        }
    }
}