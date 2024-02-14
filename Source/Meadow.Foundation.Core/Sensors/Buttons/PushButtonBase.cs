using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Buttons;

/// <summary>
/// Contains common push button logic
/// </summary>
public abstract class PushButtonBase : IButton, IDisposable
{
    private object eventSyncRoot = new();
    private EventHandler? pressStarted;
    private EventHandler? pressEnded;
    private EventHandler? clicked;

    /// <summary>
    /// Default threshold for LongClicked events
    /// </summary>
    public static readonly TimeSpan DefaultLongClickThreshold = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Raised when a press starts
    /// </summary>
    public event EventHandler? PressStarted
    {
        add
        {
            if (DigitalIn is IDigitalInterruptPort dip)
            {
                if (dip.InterruptMode == InterruptMode.None)
                {
                    throw new ArgumentException("To receive a PressStarted event, you must have an interrupt enabled");
                }
            }
            lock (eventSyncRoot)
            {
                pressStarted += value;
            }
        }
        remove
        {
            lock (eventSyncRoot)
            {
                pressStarted -= value;
            }
        }
    }

    /// <summary>
    /// Raised when a press ends
    /// </summary>
    public event EventHandler PressEnded
    {
        add
        {
            if (DigitalIn is IDigitalInterruptPort dip)
            {
                if (dip.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new ArgumentException("To receive a PressEnded event, you must have both interrupts enabled");
                }
            }
            lock (eventSyncRoot)
            {
                pressEnded += value;
            }
        }
        remove
        {
            lock (eventSyncRoot)
            {
                pressEnded -= value;
            }
        }
    }
    /// <summary>
    /// Raised when the button is released after being pressed (for shorter than LongClickedThreshold, if set)
    /// </summary>
    public event EventHandler Clicked
    {
        add
        {
            if (DigitalIn is IDigitalInterruptPort dip)
            {
                if (dip.InterruptMode != InterruptMode.EdgeBoth)
                {
                    throw new ArgumentException("To receive a Clicked event, you must have both interrupts enabled");
                }
            }
            lock (eventSyncRoot)
            {
                clicked += value;
            }
        }
        remove
        {
            lock (eventSyncRoot)
            {
                clicked -= value;
            }
        }
    }

    /// <summary>
    /// Raised when the button is released after being pressed for longer than LongClickedThreshold
    /// </summary>
    public event EventHandler LongClicked = default!;

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
    /// The minimum duration for a long press. Defaults to 
    /// </summary>
    public TimeSpan LongClickedThreshold { get; set; } = DefaultLongClickThreshold;

    /// <summary>
    /// Initializes a new instance of the PushButtonBase class with the specified digital input port
    /// </summary>
    /// <param name="inputPort">The digital input port to associate with the push button</param>
    protected PushButtonBase(IDigitalInputPort inputPort)
    {
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
        clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raised when a press starts (the button is pushed down; circuit is closed).
    /// </summary>
    protected virtual void RaisePressStarted()
    {
        pressStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raised when a press ends (the button is released; circuit is opened).
    /// </summary>
    protected virtual void RaisePressEnded()
    {
        pressEnded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raised when the button circuit is pressed for at least 500ms.
    /// </summary>
    protected virtual void RaiseLongClicked()
    {
        LongClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Convenience method to get the current sensor reading
    /// </summary>
    public Task<bool> Read() => Task.FromResult(State);

    ///<inheritdoc/>
    public virtual void Dispose()
    {
        if (ShouldDisposeInput)
        {
            DigitalIn.Dispose();
        }
    }
}