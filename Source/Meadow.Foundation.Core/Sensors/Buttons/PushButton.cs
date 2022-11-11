using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Buttons
{
    /// <summary>
    /// A simple push button. 
    /// </summary>
    public class PushButton : IButton, IDisposable
    { 
        /// <summary>
        /// Default Debounce used on the PushButton Input if an InputPort is auto-created
        /// </summary>
        public static readonly TimeSpan DefaultDebounceDuration = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Default Glitch Filter used on the PushButton Input if an InputPort is auto-created
        /// </summary>
        public static readonly TimeSpan DefaultGlitchDuration = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// Default threshold for LongPress events
        /// </summary>
        public static readonly TimeSpan DefaultLongPressThreshold = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// This duration controls the debounce filter. It also has the effect
        /// of rate limiting clicks. Decrease this time to allow users to click
        /// more quickly.
        /// </summary>
        public TimeSpan DebounceDuration
        {
            get => (DigitalIn != null) ? DigitalIn.DebounceDuration : TimeSpan.MinValue;
            set => DigitalIn.DebounceDuration = value;
        }

        /// <summary>
        /// The button state polling interval for PushButton instances that are created
        /// from a port that doesn't have an tnterrupt mode of EdgeBoth - otherwise ignored
        /// </summary>
        public TimeSpan ButtonPollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Returns the sanitized state of the switch. If the switch 
        /// is pressed, returns true, otherwise false.
        /// </summary>
        public bool State
        {
            get
            {
                bool currentState = DigitalIn?.Resistor == ResistorMode.InternalPullDown;
                return rawState == currentState;
            }
        }

        /// <summary>
        /// The minimum duration for a long press
        /// </summary>
        public TimeSpan LongClickedThreshold { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Returns digital input port
        /// </summary>
        protected IDigitalInputPort DigitalIn { get; set; }

        /// <summary>
        /// Raised when a press starts (the button is pushed down - circuit is closed)
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when a press ends (the button is released - circuit is opened)
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when the button circuit is re-opened after it has been closed (at the end of a press)
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when the button is pressed for LongClickedThreshold or longer
        /// </summary>
        public event EventHandler LongClicked = delegate { };

        /// <summary>
        /// Returns the current raw state of the switch
        /// </summary>
        protected bool rawState => (DigitalIn != null) && !DigitalIn.State;

        /// <summary>
        /// Maximum DateTime value when the button was just pushed
        /// </summary>
        protected DateTime buttonPressStart = DateTime.MaxValue;

        /// <summary>
        /// The button port resistor mode
        /// </summary>
        protected ResistorMode resistorMode => DigitalIn.Resistor;

        /// <summary>
        /// Cancellation token source to disable button polling on dispose
        /// </summary>
        protected CancellationTokenSource? ctsPolling;

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool shouldDisposeInput = false;

        /// <summary>
        /// Creates PushButton with a digital input pin connected on a IIOdevice, specifying if its using an Internal or External PullUp/PullDown resistor.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        /// <param name="resistorMode"></param>
        public PushButton(IDigitalInputController device, IPin inputPin, ResistorMode resistorMode = ResistorMode.InternalPullUp)
            : this(device.CreateDigitalInputPort(inputPin, InterruptMode.EdgeBoth, resistorMode, DefaultDebounceDuration, DefaultGlitchDuration))
        {  
            shouldDisposeInput = true;
        }

        /// <summary>
        /// Creates PushButton with a pre-configured input port
        /// </summary>
        /// <param name="inputPort"></param>
        public PushButton(IDigitalInputPort inputPort)
        {
            DigitalIn = inputPort;

            LongClickedThreshold = DefaultLongPressThreshold;

            if (DigitalIn.InterruptMode == InterruptMode.EdgeBoth)
            {
                DigitalIn.Changed += DigitalInChanged;
            }
            else
            {   //if we don't have full interrupts, fall back to polling
                //Re set the resistor mode if Interrupt mode is none - ToDo - known issue
                if ((DigitalIn.InterruptMode == InterruptMode.None && resistorMode == ResistorMode.InternalPullUp) ||
                    (DigitalIn.InterruptMode == InterruptMode.None && resistorMode == ResistorMode.InternalPullDown))
                { DigitalIn.Resistor = resistorMode; }

                ctsPolling = new CancellationTokenSource();

                bool currentState = DigitalIn.State;
               
                _ = Task.Run(async () =>
                {
                    while(!ctsPolling.Token.IsCancellationRequested)
                    {
                        if (currentState != DigitalIn.State)
                        {
                            UpdateEvents(currentState = DigitalIn.State);
                        }

                        await Task.Delay(ButtonPollingInterval);
                    }
                });
            }
        }

        void DigitalInChanged(object sender, DigitalPortResult result)
        {
            UpdateEvents(rawState);
        }

        void UpdateEvents(bool state)
        {
            state = (resistorMode == ResistorMode.InternalPullUp ||
              resistorMode == ResistorMode.ExternalPullUp) ? !state : state;

            if (state)
            {
                buttonPressStart = DateTime.Now;
                RaisePressStarted();
            }
            else
            {
                TimeSpan pressDuration = DateTime.Now - buttonPressStart;

                buttonPressStart = DateTime.MaxValue;

                if (LongClickedThreshold > TimeSpan.Zero && pressDuration > LongClickedThreshold)
                {
                    RaiseLongClicked();
                }
                else
                {
                    RaiseClicked();
                }
                RaisePressEnded();
            }
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

        /// <summary>
        /// Disposes the Digital Input resources
        /// </summary>
        public void Dispose()
        {
            if (shouldDisposeInput)
            {
                DigitalIn.Dispose();
            }

            ctsPolling?.Cancel();
        }
    }
}