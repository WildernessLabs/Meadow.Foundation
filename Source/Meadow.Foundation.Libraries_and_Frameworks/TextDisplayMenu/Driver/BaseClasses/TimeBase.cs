using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class TimeBase : InputBase
    {
        int[] timeParts;
        byte position = 0;

        protected TimeMode timeMode;

        public override event ValueChangedHandler ValueChanged;

        public TimeBase(TimeMode timeMode)
        {
            this.timeMode = timeMode;
        }

        string TimeDisplay
        {
            get
            {
                string value = string.Empty;
                for (int i = 0; i < timeParts.Length; i++)
                {
                    if (i > 0) { value += ":"; }
                    value += InputHelpers.PadLeft(timeParts[i].ToString(), '0', 2);
                }
                return InputHelpers.PadLeft(value, ' ', display.DisplayConfig.Width);
            }
        }

        string TimeModeDisplay
        {
            get
            {
                switch (timeMode)
                {
                    case TimeMode.HH_MM_SS: return "hh:mm:ss";
                    case TimeMode.HH_MM: return "hh:mm";
                    case TimeMode.MM_SS: return "mm:ss";
                    default: throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// Time mode for the input, HH:MM:SS, HH:MM, or MM:SS
        /// </summary>
        public enum TimeMode
        {
            HH_MM_SS,
            HH_MM,
            MM_SS
        }

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

            timeParts = new int[timeMode == TimeMode.HH_MM_SS ? 3 : 2];
            base.itemID = itemID;
            display.ClearLines();
            display.WriteLine("Enter " + TimeModeDisplay, 0);
            
            //display.SetCursorPosition(0, 0);

            ParseValue(currentValue);
            UpdateInputLine(TimeDisplay);
        }

        public override bool Previous()
        {
            int max = 0;

            if (position == 0)
            {
                if (timeMode == TimeMode.HH_MM_SS) { max = 23; }
                if (timeMode == TimeMode.HH_MM) { max = 23; }
                if (timeMode == TimeMode.MM_SS) { max = 59; }
            }
            else
            {
                max = 59;
            }

            if (timeParts[position] < max) { timeParts[position]++; }
            UpdateInputLine(TimeDisplay);

            return true;
        }


        public override bool Next()
        {
            int min = 0;
            if (timeParts[position] > min) { timeParts[position]--; }
            UpdateInputLine(TimeDisplay);

            return true;
        }

        public override bool Select()
        {
            if (position < timeParts.Length - 1)
            {
                position++;
            }
            else
            {
                TimeSpan timeSpan;

                switch (timeMode)
                {
                    case TimeMode.HH_MM_SS:
                        timeSpan = new TimeSpan(timeParts[0], timeParts[1], timeParts[2]);
                        break;
                    case TimeMode.HH_MM:
                        timeSpan = new TimeSpan(timeParts[0], timeParts[1], 0);
                        break;
                    case TimeMode.MM_SS:
                        timeSpan = new TimeSpan(0, timeParts[0], timeParts[1]);
                        break;
                    default: throw new ArgumentException();
                }
                ValueChanged(this, new ValueChangedEventArgs(itemID, timeSpan));
            }

            return true;
        }

        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            if(value is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)value;
                switch (timeMode)
                {
                    case TimeMode.HH_MM_SS:
                        timeParts[0] = ts.Hours;
                        timeParts[1] = ts.Minutes;
                        timeParts[2] = ts.Seconds;
                        break;
                    case TimeMode.HH_MM:
                        timeParts[0] = ts.Hours;
                        timeParts[1] = ts.Minutes;
                        break;
                    case TimeMode.MM_SS:
                        timeParts[0] = ts.Minutes;
                        timeParts[1] = ts.Seconds;
                        break;
                    default: throw new ArgumentException();
                }
            }
            else
            {
                string currentValue = value.ToString();

                var parts = currentValue.Split(new char[] { ':' });

                for (int i = 0; i < parts.Length; i++)
                {
                    timeParts[i] = int.Parse(parts[i]);
                }
            }
        }
    }
}