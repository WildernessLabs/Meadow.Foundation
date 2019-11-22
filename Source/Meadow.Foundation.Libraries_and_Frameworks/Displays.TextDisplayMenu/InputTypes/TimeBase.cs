using Meadow.Peripherals.Sensors.Rotary;
using System;

namespace Meadow.Foundation.Displays.TextDisplayMenu.InputTypes
{
    public abstract class TimeBase : InputBase
    {
        int[] _timeParts;
        byte _pos = 0;

        protected TimeMode _timeMode;

        public override event ValueChangedHandler ValueChanged;

        public TimeBase(TimeMode timeMode)
        {
            _timeMode = timeMode;
        }

        string TimeDisplay
        {
            get
            {
                string value = string.Empty;
                for (int i = 0; i < _timeParts.Length; i++)
                {
                    if (i > 0) value += ":";
                    value += InputHelpers.PadLeft(_timeParts[i].ToString(), '0', 2);
                }
                return InputHelpers.PadLeft(value, ' ', _display.DisplayConfig.Width);
            }
        }

        string TimeModeDisplay
        {
            get
            {
                switch (_timeMode)
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
            if (!_isInit)
            {
                throw new InvalidOperationException("Init() must be called before getting input.");
            }

            _timeParts = new int[_timeMode == TimeMode.HH_MM_SS ? 3 : 2];
            _itemID = itemID;
            _display.Clear();
            _display.WriteLine("Enter " + this.TimeModeDisplay, 0);
            _display.SetCursorPosition(0, 1);

            RegisterHandlers();
            ParseValue(currentValue);
            RewriteInputLine(TimeDisplay);
        }

        protected override void HandlePrevious(object sender, EventArgs e)
        {
            DoPrevious();
        }

        protected override void HandleNext(object sender, EventArgs e)
        {
            DoNext();
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
            int max = 0;

            if (_pos == 0)
            {
                if (_timeMode == TimeMode.HH_MM_SS) max = 23;
                if (_timeMode == TimeMode.HH_MM) max = 23;
                if (_timeMode == TimeMode.MM_SS) max = 59;
            }
            else
            {
                max = 59;
            }

            if (_timeParts[_pos] < max) _timeParts[_pos]++;
            RewriteInputLine(TimeDisplay);
        }


        private void DoPrevious()
        {
            int min = 0;
            if (_timeParts[_pos] > min) _timeParts[_pos]--;
            RewriteInputLine(TimeDisplay);
        }

        protected override void HandleClicked(object sender, EventArgs e)
        {
            if (_pos < _timeParts.Length - 1)
            {
                _pos++;
            }
            else
            {
                UnregisterHandlers();

                TimeSpan timeSpan;

                switch (_timeMode)
                {
                    case TimeMode.HH_MM_SS:
                        timeSpan = new TimeSpan(_timeParts[0], _timeParts[1], _timeParts[2]);
                        break;
                    case TimeMode.HH_MM:
                        timeSpan = new TimeSpan(_timeParts[0], _timeParts[1], 0);
                        break;
                    case TimeMode.MM_SS:
                        timeSpan = new TimeSpan(0, _timeParts[0], _timeParts[1]);
                        break;
                    default: throw new ArgumentException();
                }
                ValueChanged(this, new ValueChangedEventArgs(_itemID, timeSpan));
            }
        }

        protected override void ParseValue(object value)
        {
            if (value == null || value.ToString() == string.Empty) return;

            if(value is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)value;
                switch (_timeMode)
                {
                    case TimeMode.HH_MM_SS:
                        _timeParts[0] = ts.Hours;
                        _timeParts[1] = ts.Minutes;
                        _timeParts[2] = ts.Seconds;
                        break;
                    case TimeMode.HH_MM:
                        _timeParts[0] = ts.Hours;
                        _timeParts[1] = ts.Minutes;
                        break;
                    case TimeMode.MM_SS:
                        _timeParts[0] = ts.Minutes;
                        _timeParts[1] = ts.Seconds;
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
                    _timeParts[i] = int.Parse(parts[i]);
                }
            }
        }
    }
}
