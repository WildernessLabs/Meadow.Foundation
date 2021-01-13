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
        event EventHandler clickDelegate = delegate { };
        event EventHandler pressStartDelegate = delegate { };
        event EventHandler pressEndDelegate = delegate { };
        event EventHandler longPressDelegate = delegate { };

        /// <summary>
        /// Returns the sanitized state of the switch. If the switch 
        /// is pressed, returns true, otherwise false.
        /// </summary>
        public bool State
        {
            get
            {
                bool currentState = DigitalIn?.Resistor == ResistorMode.PullDown ? true : false;

                return (state == currentState) ? true : false;
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
                if (DigitalIn.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new Exception("PressEnded event requires InterruptMode.EdgeBoth");
                }

                pressStartDelegate += value;
            }
            remove => pressStartDelegate -= value;
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

                pressEndDelegate += value;
            }
            remove => pressEndDelegate -= value;
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
                clickDelegate += value;
            }
            remove => clickDelegate -= value;
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
                longPressDelegate += value;
            }
            remove => longPressDelegate -= value;
        }

        /// <summary>
        /// Returns the current raw state of the switch.
        /// </summary>
        protected bool state => (DigitalIn != null) ? !DigitalIn.State : false;

        /// <summary>
        /// Maximum DateTime value when the button was just pushed
        /// </summary>
        protected DateTime buttonPressStart = DateTime.MaxValue;

        /// <summary>
        /// Resistor Type to indicate if 
        /// </summary>
        protected ResistorType resistorType;

        /// <summary>
        /// Creates PushButto a digital input port connected on a IIOdevice, especifying Interrupt Mode, Circuit Type and optionally Debounce filter duration.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistor"></param>
        /// <param name="debounceDuration"></param>
        [Obsolete("[4.X] Constructor Deprecated. Please a constructor that uses the ResistorType parameter.")]
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
        [Obsolete("[4.X] Constructor Deprecated. Please a constructor that uses the ResistorType parameter.")]
        public PushButton(IDigitalInputPort interruptPort, ResistorMode resistor = ResistorMode.Disabled, int debounceDuration = 20)
        {
            DigitalIn = interruptPort;
            DigitalIn.Resistor = resistor;
            DigitalIn.DebounceDuration = debounceDuration;
            DigitalIn.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Creates PushButton with a digital input pin connected on a IIOdevice, especifying if its using an Internal or External PullUp/PullDown resistor.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistorType"></param>
        public PushButton(IIODevice device, IPin inputPin, ResistorType resistorType = ResistorType.ExternalPullUp) 
            : this(device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, ResistorMode.Disabled, 50, 25), resistorType) { }

        /// <summary>
        /// Creates PushButton with a digital input port, especifying if its using an Internal or External PullUp/PullDown resistor.
        /// </summary>
        /// <param name="interruptPort"></param>
        /// <param name="resistorType"></param>
        public PushButton(IDigitalInputPort interruptPort, ResistorType resistorType = ResistorType.ExternalPullUp)
        {
            this.resistorType = resistorType;
            DigitalIn = interruptPort;

            switch (resistorType)
            {
                case ResistorType.InternalPullUp:
                    DigitalIn.Resistor = ResistorMode.PullUp;
                    break;
                case ResistorType.InternalPullDown:
                    DigitalIn.Resistor = ResistorMode.PullDown;
                    break;
                case ResistorType.ExternalPullUp:
                    DigitalIn.Resistor = ResistorMode.Disabled;
                    break;
                case ResistorType.ExternalPulldown:
                    DigitalIn.Resistor = ResistorMode.Disabled;
                    break;
            }

            DigitalIn.Changed += DigitalInChanged;
        }

        void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            bool state = (resistorType == ResistorType.InternalPullUp || 
                          resistorType == ResistorType.ExternalPullUp) ? !e.Value : e.Value;

            //Console.WriteLine($"PB: InputChanged. State == {State}. e.Value: {e.Value}.  DI State: {DigitalIn.State}");

            if (state)
            {
                RaiseClicked();

                // save our press start time (for long press event)
                buttonPressStart = DateTime.Now;
                // raise our event in an inheritance friendly way
                RaisePressStarted();
            }
            else 
            {
                // calculate the press duration
                TimeSpan pressDuration = DateTime.Now - buttonPressStart;

                // reset press start time
                buttonPressStart = DateTime.MaxValue;

                // if it's a long press, raise our long press event
                if (LongPressThreshold > TimeSpan.Zero && pressDuration > LongPressThreshold)
                {
                    RaiseLongPress();
                }

                if (pressDuration.TotalMilliseconds > 0)
                {
                    // raise the other events
                    RaisePressEnded();
                }
            }
        }

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a �press�).
        /// </summary>
        protected virtual void RaiseClicked()
        {
            clickDelegate(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            pressStartDelegate(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            pressEndDelegate(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongPress()
        {
            longPressDelegate(this, new EventArgs());
        }

        public void Dispose()
        {
            //if (_inputCreatedInternally)
            //{
                DigitalIn.Dispose();
                DigitalIn = null;
            //}
        }
    }
}