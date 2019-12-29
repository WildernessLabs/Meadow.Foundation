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
        #region Properties

        /// <summary>
        /// This duration controls the debounce filter. It also has the effect
        /// of rate limiting clicks. Decrease this time to allow users to click
        /// more quickly.
        /// </summary>
        public TimeSpan DebounceDuration
        {
            get => (DigitalIn != null) ? new TimeSpan(0, 0, 0, 0, DigitalIn.DebounceDuration) : TimeSpan.MinValue;
            set
            {
                DigitalIn.DebounceDuration = (int) value.TotalMilliseconds;
            }
        }

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
        public IDigitalInputPort DigitalIn { get; private set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        private EventHandler _pressStarted = delegate {};
        public event EventHandler PressStarted
        {
            add
            {
                if(DigitalIn.InterruptMode == InterruptMode.None)
                {
                    throw new ArgumentException("To receive a PressStarted event, you must have an interrupt enabled");
                }
                lock (_eventSyncRoot)
                {
                    _pressStarted += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _pressStarted -= value;
                }
            }
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        private EventHandler _pressEnded = delegate { };
        public event EventHandler PressEnded
        {
            add
            {
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new ArgumentException("To receive a PressEnded event, you must have both interrupts enabled");
                }
                lock (_eventSyncRoot)
                {
                    _pressEnded += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _pressEnded -= value;
                }
            }
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�.
        /// </summary>
        private EventHandler _clicked = delegate { };
        public event EventHandler Clicked
        {
            add
            {
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new ArgumentException("To receive a Clicked event, you must have both interrupts enabled");
                }
                lock (_eventSyncRoot)
                {
                    _clicked += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _clicked -= value;
                }
            }
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        private EventHandler _longPressClicked = delegate { };
        public event EventHandler LongPressClicked
        {
            add
            {
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new ArgumentException("To receive a LongPressClicked event, you must have both interrupts enabled");
                }
                lock (_eventSyncRoot)
                {
                    _longPressClicked += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _longPressClicked -= value;
                }
            }
        }


        #endregion

        #region Member variables / fields

        private object _eventSyncRoot = new object();

        /// <summary>
        /// Minimum DateTime value when the button was pushed
        /// </summary>
        protected DateTime _lastClicked = DateTime.MinValue;

        /// <summary>
        /// Maximum DateTime value when the button was just pushed
        /// </summary>
        protected DateTime _buttonPressStart = DateTime.MaxValue;

        /// <summary>
        /// Circuit Termination Type (CommonGround, High or Floating)
        /// </summary>
        protected CircuitTerminationType _circuitType;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private PushButton() { }

        /// <summary>
        /// Creates PushButto a digital input port connected on a IIOdevice, especifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="debounceDuration"></param>
        public PushButton(IIODevice device, IPin inputPin, int debounceDuration = 20)
        {
            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            var resistorMode = ResistorMode.Disabled;            

            DigitalIn = device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, resistorMode, debounceDuration);
            DigitalIn.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Creates a PushButton on a digital input portespecifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="interruptPort"></param>
        /// <param name="debounceDuration"></param>
        public PushButton(IDigitalInputPort interruptPort, int debounceDuration = 20) 
		{
            DigitalIn = interruptPort;
            DebounceDuration = new TimeSpan(0, 0, 0, 0, debounceDuration);
            DigitalIn.Changed += DigitalInChanged;
        }

        #endregion

        #region Methods

        private void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            //// check how much time has elapsed since the last click
            //var timeSinceLast = DateTime.Now - _lastClicked;
            //if (timeSinceLast <= DebounceDuration)
            //{
            //    return;
            //}
            //_lastClicked = DateTime.Now;

            bool STATE_PRESSED = _circuitType == CircuitTerminationType.High ? true : false;
            bool STATE_RELEASED = _circuitType == CircuitTerminationType.High ? false : true;
            
            if(State == STATE_PRESSED)
            {
                // save our press start time (for long press event)
                _buttonPressStart = DateTime.Now;
                // raise our event in an inheritance friendly way
                this.RaisePressStarted();
            }
            else if(State == STATE_RELEASED)
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
            }
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�).
        /// </summary>
        protected virtual void RaiseClicked ()
		{
			this._clicked (this, EventArgs.Empty);
		}

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            // raise the press started event
            this._pressStarted(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            this._pressEnded(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongPress()
        {
            this._longPressClicked(this, new EventArgs());
        }

        #endregion
    }
}