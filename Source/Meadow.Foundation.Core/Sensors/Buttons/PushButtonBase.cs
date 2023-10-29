using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Buttons
{
    /// <summary>
    /// Contains common push button logic
    /// </summary>
    public abstract class PushButtonBase : IButton, IDisposable
    {
        /// <summary>
        /// Default threshold for LongPress events
        /// </summary>
        public static readonly TimeSpan DefaultLongPressThreshold = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Raised when a press starts
        /// </summary>
        public event EventHandler PressStarted = delegate { };

        /// <summary>
        /// Raised when a press ends
        /// </summary>
        public event EventHandler PressEnded = delegate { };

        /// <summary>
        /// Raised when the button is released after a press
        /// </summary>
        public event EventHandler Clicked = delegate { };

        /// <summary>
        /// Raised when the button is pressed for LongClickedThreshold or longer and then releases
        /// </summary>
        public event EventHandler LongClicked = delegate { };

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposeInput { get; set; } = false;

        /// <summary>
        /// The date/time when the last button press occurred and the button hasn't been released
        /// </summary>
        protected DateTime ButtonPressStart { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// The digital input port used by the button
        /// </summary>
        protected IDigitalInputPort DigitalIn { get; private set; }

        /// <summary>
        /// The minimum duration for a long press
        /// </summary>
        public TimeSpan LongClickedThreshold { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Initializes a new instance of the PushButtonBase class with the specified digital input port
        /// </summary>
        /// <param name="inputPort">The digital input port to associate with the push button</param>
        protected PushButtonBase(IDigitalInputPort inputPort)
        {
            LongClickedThreshold = DefaultLongPressThreshold;
            DigitalIn = inputPort;
        }

        /// <summary>
        /// Returns the sanitized state of the button
        /// If pressed, return true, otherwise false
        /// </summary>
        public bool State => GetNormalizedState(DigitalIn.State);

        /// <summary>
        /// Returns the sanitized state of the button
        /// Inverts the state when using a pull-up resistor
        /// </summary>
        protected bool GetNormalizedState(bool state)
        {
            return DigitalIn.Resistor switch
            {
                ResistorMode.ExternalPullUp or ResistorMode.InternalPullUp => !state,
                _ => state,
            };
        }

        /// <summary>
        /// Raises the proper button events based on current and previous states
        /// </summary>
        /// <param name="state"></param>
        protected void UpdateEvents(bool state)
        {
            if (state)
            {
                ButtonPressStart = DateTime.UtcNow;
                RaisePressStarted();
            }
            else
            {
                if (ButtonPressStart == DateTime.MaxValue)
                {
                    return;
                }

                TimeSpan pressDuration = DateTime.UtcNow - ButtonPressStart;
                ButtonPressStart = DateTime.MaxValue;

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
        /// Convenience method to get the current sensor reading
        /// </summary>
        public Task<bool> Read() => Task.FromResult(State);

        /// <summary>
        /// Disposes the Digital Input resources
        /// </summary>
        public virtual void Dispose()
        {
            if (ShouldDisposeInput)
            {
                DigitalIn.Dispose();
            }
        }
    }
}