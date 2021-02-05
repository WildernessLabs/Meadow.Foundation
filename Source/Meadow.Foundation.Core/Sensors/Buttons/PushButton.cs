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
                bool currentState = DigitalIn?.Resistor == ResistorMode.InternalPullDown ? true : false;

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
        protected ResistorMode resistorMode;

        /// <summary>
        /// Creates PushButton with a digital input pin connected on a IIOdevice, especifying if its using an Internal or External PullUp/PullDown resistor.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistorMode"></param>
        public PushButton(IIODevice device, IPin inputPin, ResistorMode resistorMode = ResistorMode.InternalPullUp) 
            : this(device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, resistorMode, 50, 25)) { }

        /// <summary>
        /// Creates PushButton with a digital input port, especifying if its using an Internal or External PullUp/PullDown resistor.
        /// </summary>
        /// <param name="interruptPort"></param>
        /// <param name="resistorMode"></param>
        public PushButton(IDigitalInputPort interruptPort)
        {
            if (interruptPort.Resistor == ResistorMode.Disabled)
            {
                throw new Exception("PushButton requires ResistorMode to be != Disabled");
            }

            resistorMode = interruptPort.Resistor;

            DigitalIn = interruptPort;            
            DigitalIn.Changed += DigitalInChanged;
        }

        void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            bool state = (resistorMode == ResistorMode.InternalPullUp || 
                          resistorMode == ResistorMode.ExternalPullUp) ? !e.Value : e.Value;

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