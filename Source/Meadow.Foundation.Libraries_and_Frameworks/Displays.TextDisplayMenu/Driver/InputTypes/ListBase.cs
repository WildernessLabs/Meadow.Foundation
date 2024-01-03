using System;

namespace Meadow.Foundation.Displays.UI.InputTypes
{
    /// <summary>
    /// Text display menu base List input type
    /// </summary>
    public abstract class ListBase : InputBase
    {
        /// <summary>
        /// List of choices for the input
        /// </summary>
        protected string[] choices;

        /// <summary>
        /// Selected index in the list
        /// </summary>
        protected int selectedIndex = 0;

        /// <summary>
        /// The value changed event handler
        /// </summary>
        public override event ValueChangedHandler ValueChanged = default!;

        /// <summary>
        /// The output display text
        /// </summary>
        protected string OutputDisplay
        {
            get
            {
                return InputHelpers.PadLeft(choices[selectedIndex], ' ', display.DisplayConfig.Width);
            }
        }

        /// <summary>
        /// Get the input
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <param name="currentValue">Current value</param>
        /// <exception cref="InvalidOperationException">Throw if not initialized</exception>
        public override void GetInput(string itemID, object currentValue)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Init() must be called before getting input.");
            }

            display.ClearLines();
            display.WriteLine("Select", 0);

            ParseValue(currentValue);
            UpdateInputLine(OutputDisplay);
        }

        /// <summary>
        /// Scroll to the next item in the list
        /// </summary>
        /// <returns>true</returns>
        public override bool Next()
        {
            if (selectedIndex < choices.Length - 1)
            {
                selectedIndex++;
                UpdateInputLine(OutputDisplay);
            }
            return true;
        }

        /// <summary>
        /// Select the current item in the list
        /// </summary>
        /// <returns>true</returns>
        public override bool Select()
        {
            ValueChanged(this, new ValueChangedEventArgs(itemID, choices[selectedIndex]));
            return true;
        }

        /// <summary>
        /// Select the current item in the list
        /// </summary>
        /// <returns>true</returns>
        public override bool Back()
        {
            ValueChanged(this, new ValueChangedEventArgs(itemID, choices[selectedIndex]));
            return true;
        }

        /// <summary>
        /// Scroll to the previous item in the list
        /// </summary>
        /// <returns>true</returns>
        public override bool Previous()
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
                UpdateInputLine(OutputDisplay);
            }
            return true;
        }

        /// <summary>
        /// Parse a value for the item
        /// </summary>
        /// <param name="value">The string value as an object</param>
        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            for (int i = 0; i < choices.Length; i++)
            {
                if (choices[i] == value.ToString())
                {
                    selectedIndex = i;
                    break;
                }
            }
        }
    }
}