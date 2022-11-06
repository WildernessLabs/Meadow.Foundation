using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    /// <summary>
    /// Text display menu base List input type
    /// </summary>
    public abstract class ListBase : InputBase
    {
        /// <summary>
        /// List choices
        /// </summary>
        protected string[] choices;

        /// <summary>
        /// Selected index in the list
        /// </summary>
        protected int selectedIndex = 0;

        /// <summary>
        /// The value changed event handler
        /// </summary>
        public override event ValueChangedHandler ValueChanged;

        string OutputDisplay
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
            display.SetCursorPosition(0, 1);

            ParseValue(currentValue);
            UpdateInputLine(OutputDisplay);
        }

        /// <summary>
        /// Send a Next input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Next()
        {
            if(selectedIndex < choices.Length - 1)
            {
                selectedIndex++;
                UpdateInputLine(OutputDisplay);
            }
            return true;
        }

        /// <summary>
        /// Send a Select input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Select()
        {
            ValueChanged(this, new ValueChangedEventArgs(itemID, choices[selectedIndex]));
            return true;
        }

        /// <summary>
        /// Send a Previous input to the item
        /// </summary>
        /// <returns>true</returns>
        public override bool Previous()
        {
            if(selectedIndex > 0)
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

            for (int i = 0; i< choices.Length; i++)
            {
                if(choices[i] == value.ToString())
                {
                    selectedIndex = i;
                    break;
                }
            }
        }
    }
}