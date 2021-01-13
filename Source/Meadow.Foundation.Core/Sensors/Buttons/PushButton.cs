using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;

namespace Meadow.Foundation.Sensors.Buttons
{
    /// <summary>
    /// A simple push button. 
    /// </summary>
    public class PushButton : IButton, IDisposable
    {
        #region Properties        

        /// <summary>
        /// Returns the sanitized state of the switch. If the switch 
        /// is pressed, returns true, otherwise false.
        /// </summary>
        public bool State
        {
            get
            {
                bool currentState = DigitalIn?.Resistor == ResistorMode.PullDown ? true : false;

                return (_state == currentState) ? true : false;
            }
        }

        /// <summary>
        /// The minimum duration for a long press.
        /// </summary>
        public TimeSpan LongPressThreshold { get; set; } = new TimeSpan(0, 0, 1);

        /// <summary>
        /// Returns digital input port.
        /// </summary>
        public IDigitalInputPort DigitalIn { get; private set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        public event EventHandler PressStarted
        {
            add
            {
                if (DigitalIn.InterruptMode == InterruptMode.None)
                {
                    throw new Exception("PressStarted event requires InterruptMode to be != None");
                }

                _pressStartDelegate += value;
            }
            remove => _pressStartDelegate -= value;
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        public event EventHandler PressEnded
        {
            add
            {
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new Exception("PressEnded event requires InterruptMode.EdgeBoth");
                }

                _pressEndDelegate += value;
            }
            remove => _pressEndDelegate -= value;
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�.
        /// </summary>
        public event EventHandler Clicked
        {
            add
            {
                if (DigitalIn.InterruptMode == InterruptMode.None)
                {
                    throw new Exception("PressStarted event requires InterruptMode to be != None");
                }
                _clickDelegate += value;
            }
            remove => _clickDelegate -= value;
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        public event EventHandler LongPressClicked
        {
            add
            {
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new Exception("LongPressClicked event requires InterruptMode.EdgeBoth");
                }
                _longPressDelegate += value;
            }
            remove => _longPressDelegate -= value;
        }
        #endregion

        #region Member variables / fields
        private event EventHandler _clickDelegate = delegate { };
        private event EventHandler _pressStartDelegate = delegate { };
        private event EventHandler _pressEndDelegate = delegate { };
        private event EventHandler _longPressDelegate = delegate { };
        //private bool _inputCreatedInternally = false;

        /// <summary>
        /// Returns the current raw state of the switch.
        /// </summary>
        protected bool _state => (DigitalIn != null) ? !DigitalIn.State : false;

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
        //protected CircuitTerminationType _circuitType;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates PushButto a digital input port connected on a IIOdevice, especifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistor"></param>
        /// <param name="debounceDuration"></param>        
        public PushButton(IIODevice device, IPin inputPin, ResistorMode resistor = ResistorMode.Disabled, int debounceDuration = 20)
        {
            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            DigitalIn = device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, resistor, debounceDuration);
            DigitalIn.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Creates a PushButton on a digital input portespecifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="interruptPort"></param>
        /// <param name="resistor"></param>
        /// <param name="debounceDuration"></param>
        public PushButton(IDigitalInputPort interruptPort, ResistorMode resistor = ResistorMode.Disabled, int debounceDuration = 20)
        {
            DigitalIn = interruptPort;
            DigitalIn.Resistor = resistor;
            DigitalIn.DebounceDuration = debounceDuration;
            DigitalIn.Changed += DigitalInChanged;
        }

        #endregion

        #region Methods

        private void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            bool STATE_PRESSED = false;
            bool STATE_RELEASED = true;

            //Console.WriteLine($"PB: InputChanged. State == {State}. e.Value: {e.Value}.  DI State: {DigitalIn.State}");

            if (State == STATE_PRESSED)
            {
                // save our press start time (for long press event)
                _buttonPressStart = DateTime.Now;
                // raise our event in an inheritance friendly way
                RaisePressStarted();
            }
            else if (State == STATE_RELEASED)
            {
                // calculate the press duration
                TimeSpan pressDuration = DateTime.Now - _buttonPressStart;

                // reset press start time
                _buttonPressStart = DateTime.MaxValue;

                // if it's a long press, raise our long press event
                if (LongPressThreshold > TimeSpan.Zero && pressDuration > LongPressThreshold)
                {
                    RaiseLongPress();
                }
                else
                {
                    if (e.Value)
                    {
                        RaiseClicked();
                    }
                }

                // raise the other events
                RaisePressEnded();
            }
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�).
        /// </summary>
        protected virtual void RaiseClicked()
        {
            _clickDelegate(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            // raise the press started event
            _pressStartDelegate(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            _pressEndDelegate(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongPress()
        {
            _longPressDelegate(this, new EventArgs());
        }

        public void Dispose()
        {
            //if (_inputCreatedInternally)
            //{
                DigitalIn.Dispose();
                DigitalIn = null;
            //}
        }

        #endregion
    }
}