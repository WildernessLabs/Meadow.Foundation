using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class ListBase : InputBase
    {
        protected string[] choices;
        protected int selectedIndex = 0;

        public override event ValueChangedHandler ValueChanged;

        string OutputDisplay
        {
            get
            {
                return InputHelpers.PadLeft(choices[selectedIndex], ' ', display.DisplayConfig.Width);
            }
        }

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

        public override bool Next()
        {
            if(selectedIndex < choices.Length - 1)
            {
                selectedIndex++;
                UpdateInputLine(OutputDisplay);
            }
            return true;
        }

        public override bool Select()
        {
            ValueChanged(this, new ValueChangedEventArgs(itemID, choices[selectedIndex]));
            return true;
        }

        public override bool Previous()
        {
            if(selectedIndex > 0)
            {
                selectedIndex--;
                UpdateInputLine(OutputDisplay);
            }
            return true;
        }

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