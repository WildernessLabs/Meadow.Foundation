using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;

namespace Meadow.Foundation.Sensors.Buttons
{
	/// <summary>
	/// A simple push button. 
	/// </summary>
	public class PushButton : IButton
	{
        protected DateTime _lastClicked = DateTime.MinValue;
        protected DateTime _buttonPressStart = DateTime.MaxValue;
        protected CircuitTerminationType _circuitType;

        /// <summary>
        /// This duration controls the debounce filter. It also has the effect
        /// of rate limiting clicks. Decrease this time to allow users to click
        /// more quickly.
        /// </summary>
        public TimeSpan DebounceDuration { get; set; }

        /// <summary>
        /// Returns the current raw state of the switch. If the switch 
        /// is pressed (connected), returns true, otherwise false.
        /// </summary>
        public bool State => (DigitalIn != null) ? !DigitalIn.State : false;

        /// <summary>
        /// The minimum duration for a long press.
        /// </summary>
        public TimeSpan LongPressThreshold { get; set; } = new TimeSpan(0, 0, 0, 0, 500);

        /// <summary>
        /// Returns digital input port.
        /// </summary>
        public DigitalInputPort DigitalIn { get; private set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a “press”.
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        public event EventHandler LongPressClicked = delegate { };

        /// <summary>
        /// Creates a Push on a digital input port, especifying Circuit type, and optionally Debounce filter duration.
        /// </summary>
        /// <param name="inputPin"></param>
        /// <param name="type"></param>
        /// <param name="debounceDuration">in milliseconds</param>
		public PushButton(IDigitalPin inputPin, CircuitTerminationType type, int debounceDuration = 20) 
		{
            _circuitType = type;
            DebounceDuration = new TimeSpan(0, 0, 0, 0, debounceDuration);

            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            var resistorMode = ResistorMode.Disabled;
            switch (type)
            {
                case CircuitTerminationType.CommonGround:
                    resistorMode = ResistorMode.PullUp;
                    break;
                case CircuitTerminationType.High:
                    resistorMode = ResistorMode.PullDown;
                    break;
                case CircuitTerminationType.Floating:
                    resistorMode = ResistorMode.Disabled;
                    break;
            } 

            // create the interrupt port from the pin and resistor type
            DigitalIn = new DigitalInputPort(inputPin, true, resistorMode);

            // wire up the interrupt handler
            DigitalIn.Changed += DigitalInChanged;
 		}

        private void DigitalInChanged(object sender, PortEventArgs e)
        {
              // check how much time has elapsed since the last click
            var timeSinceLast = DateTime.Now - _lastClicked;
            if (timeSinceLast <= DebounceDuration)
            {
                return;
            }
            _lastClicked = DateTime.Now;

            int STATE_PRESSED = _circuitType == CircuitTerminationType.High ? 1 : 0;
            int STATE_RELEASED = _circuitType == CircuitTerminationType.High ? 0 : 1;
            /* Port: TODO
            if(state == STATE_PRESSED)
            {
                // save our press start time (for long press event)
                _buttonPressStart = DateTime.Now;
                // raise our event in an inheritance friendly way
                this.RaisePressStarted();
            }
            else if(state == STATE_RELEASED)
            {
                // calculate the press duration
                TimeSpan pressDuration = DateTime.Now - _buttonPressStart;

                // reset press start time
                _buttonPressStart = DateTime.MaxValue;

                // if it's a long press, raise our long press event
                if (pressDuration > LongPressThreshold) this.RaiseLongPress();

                // raise the other events
                this.RaisePressEnded();
                this.RaiseClicked();
            } */
        }

		protected virtual void RaiseClicked ()
		{
			this.Clicked (this, EventArgs.Empty);
		}

        protected virtual void RaisePressStarted()
        {
            // raise the press started event
            this.PressStarted(this, new EventArgs());
        }

        protected virtual void RaisePressEnded()
        {
            this.PressEnded(this, new EventArgs());
        }

        protected virtual void RaiseLongPress()
        {
            this.LongPressClicked(this, new EventArgs());
        }
	}
}