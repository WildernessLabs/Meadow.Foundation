using Meadow.Peripherals.Sensors.Buttons;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a AS1115 key scan button
    /// </summary>
    public class KeyScanButton : IButton
    {
        /// <summary>
        /// Get or set the long click threshold
        /// </summary>
        public TimeSpan LongClickedThreshold { get; set; }

        /// <summary>
        /// Get current button state
        /// </summary>
        public bool State { get; private set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down)
        /// </summary>
        public event EventHandler PressStarted;

        /// <summary>
        /// Raised when a press ends (the button is released)
        /// </summary>
        public event EventHandler PressEnded;

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a press)
        /// </summary>
        public event EventHandler Clicked;

        /// <summary>
        /// Raised when the button circuit is pressed for LongPressDuration
        /// </summary>
        public event EventHandler LongClicked;

        /// <summary>
        /// Maximum DateTime value when the button was just pushed
        /// </summary>
        protected DateTime buttonPressStart = DateTime.MaxValue;

        /// <summary>
        /// Update the button state
        /// true for pressed, false for released
        /// </summary>
        /// <param name="state"></param>
        public void Update(bool state)
        {
            if (state == true && State == false)
            {   // save our press start time (for long press event)
                buttonPressStart = DateTime.Now;

                RaisePressStarted();
            }
            else if (state == false && State == true)
            {   // calculate the press duration
                TimeSpan pressDuration = DateTime.Now - buttonPressStart;

                // reset press start time
                buttonPressStart = DateTime.MaxValue;

                // if it's a long press, raise our long press event
                if (LongClickedThreshold > TimeSpan.Zero && pressDuration > LongClickedThreshold)
                {
                    RaiseLongClicked();
                }
                else
                {
                    RaiseClicked();
                }

                if (pressDuration.TotalMilliseconds > 0)
                {   // raise the other events
                    RaisePressEnded();
                }
            }

            State = state;
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�).
        /// </summary>
        protected virtual void RaiseClicked()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            PressStarted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            PressEnded?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongClicked()
        {
            LongClicked?.Invoke(this, new EventArgs());
        }
    }
}