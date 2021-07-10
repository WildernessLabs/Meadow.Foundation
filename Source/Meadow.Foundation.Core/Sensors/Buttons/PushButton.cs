using Meadow.Devices;
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
        /// <summary>
        /// This duration controls the debounce filter. It also has the effect
        /// of rate limiting clicks. Decrease this time to allow users to click
        /// more quickly.
        /// </summary>
        public TimeSpan DebounceDuration
        {
            get => (DigitalIn != null) ? new TimeSpan(0, 0, 0, 0, (int)DigitalIn.DebounceDuration) : TimeSpan.MinValue;
            set
            {
                DigitalIn.DebounceDuration = (int)value.TotalMilliseconds;
            }
        }

        event EventHandler clickDelegate = delegate { };
        event EventHandler pressStartDelegate = delegate { };
        event EventHandler pressEndDelegate = delegate { };
        event EventHandler longClickDelegate = delegate { };

        /// <summary>
        /// Returns the sanitized state of the switch. If the switch 
        /// is pressed, returns true, otherwise false.
        /// </summary>
        public bool State
        {
            get
            {
                bool currentState = DigitalIn?.Resistor == ResistorMode.InternalPullDown;

                return (state == currentState);
            }
        }

        /// <summary>
        /// The minimum duration for a long press.
        /// </summary>
        public TimeSpan LongClickedThreshold { get; set; } = TimeSpan.Zero;

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
                    throw new DeviceConfigurationException("PressStarted event requires InteruptMode to be anything but None");
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
                if (DigitalIn.InterruptMode == InterruptMode.None)
                {
                    throw new DeviceConfigurationException("PressEnded event requires InteruptMode to be anything but None");
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
                    throw new DeviceConfigurationException("Clicked event requires InteruptMode to be anything but None");
                }

                clickDelegate += value;
            }
            remove => clickDelegate -= value;
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        public event EventHandler LongClicked
        {
            add
            {
                if (DigitalIn.InterruptMode == InterruptMode.None)
                {
                    throw new DeviceConfigurationException("LongPressClicked event requires InteruptMode to be anything but None");
                }

                longClickDelegate += value;
            }
            remove => longClickDelegate -= value;
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
        public PushButton(IDigitalInputController device, IPin inputPin, ResistorMode resistorMode = ResistorMode.InternalPullUp) 
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

        void DigitalInChanged(object sender, DigitalPortResult result)
        {
            bool state = (resistorMode == ResistorMode.InternalPullUp || 
                          resistorMode == ResistorMode.ExternalPullUp) ? !result.New.State : result.New.State;

            //Console.WriteLine($"PB: InputChanged. State == {State}. result.New.State: {result.New.State}.  DI State: {DigitalIn.State}");

            if (state)
            {                
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
                if (LongClickedThreshold > TimeSpan.Zero && pressDuration > LongClickedThreshold)
                {
                    RaiseLongClicked();
                }
                else 
                {
                    RaiseClicked();
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
            clickDelegate?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raised when a press starts (the button is pushed down; circuit is closed).
        /// </summary>
        protected virtual void RaisePressStarted()
        {
            pressStartDelegate?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raised when a press ends (the button is released; circuit is opened).
        /// </summary>
        protected virtual void RaisePressEnded()
        {
            pressEndDelegate?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the button circuit is pressed for at least 500ms.
        /// </summary>
        protected virtual void RaiseLongClicked()
        {
            longClickDelegate?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            DigitalIn.Dispose();
        }
    }
}