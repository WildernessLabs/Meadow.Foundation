using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Rotary;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Rotary;

/// <summary>
/// Digital rotary encoder that uses two-bit Gray Code to encode rotation and has an integrated push button
/// </summary>
public class RotaryEncoderWithButton : RotaryEncoder, IRotaryEncoderWithButton
{
    /// <summary>
    /// Returns the PushButton that represents the integrated button
    /// </summary>
    public PushButton Button { get; private set; }

    /// <summary>
    /// Returns the push button's state
    /// </summary>
    public bool State => Button.State;

    /// <summary>
    /// Raised when the button circuit is re-opened after it has been closed
    /// </summary>
    public event EventHandler Clicked = delegate { };

    /// <summary>
    /// Raised when a press ends
    /// </summary>
    public event EventHandler PressEnded = delegate { };

    /// <summary>
    /// Raised when a press starts
    /// </summary>
    public event EventHandler PressStarted = delegate { };

    /// <summary>
    /// Raised when the button circuit is pressed for LongPressDuration
    /// </summary>
    public event EventHandler LongClicked = delegate { };

    /// <summary>
    /// The minimum duration for a long press
    /// </summary>
    public TimeSpan LongClickedThreshold
    {
        get => Button.LongClickedThreshold;
        set => Button.LongClickedThreshold = value;
    }

    /// <summary>
    /// Instantiates a new RotaryEncoder on the specified pins that has an integrated button.
    /// </summary>
    /// <param name="aPhasePin"></param>
    /// <param name="bPhasePin"></param>
    /// <param name="buttonPin"></param>
    /// <param name="buttonResistorMode"></param>
    public RotaryEncoderWithButton(IPin aPhasePin, IPin bPhasePin, IPin buttonPin, ResistorMode buttonResistorMode = ResistorMode.InternalPullDown)
        : base(aPhasePin, bPhasePin)
    {
        Button = new PushButton(buttonPin, buttonResistorMode);

        Button.Clicked += ButtonClicked;
        Button.PressEnded += ButtonPressEnded;
        Button.PressStarted += ButtonPressStarted;
        Button.LongClicked += ButtonLongClicked;
    }

    private void ButtonLongClicked(object sender, EventArgs e)
    {
        LongClicked(this, e);
    }

    /// <summary>
    /// Method when button is clicked (down then up)
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="e">event arguments</param>
    protected void ButtonClicked(object sender, EventArgs e)
    {
        Clicked(this, e);
    }

    /// <summary>
    /// Method called when button press is started (up state) 
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="e">event arguments</param>
    protected void ButtonPressEnded(object sender, EventArgs e)
    {
        PressEnded(this, e);
    }

    /// <summary>
    /// Method called when button press is started (down state) 
    /// </summary>
    /// <param name="sender">sender object</param>
    /// <param name="e">event arguments</param>
    protected void ButtonPressStarted(object sender, EventArgs e)
    {
        PressStarted(this, e);
    }

    /// <summary>
    /// Convenience method to get the current sensor reading
    /// </summary>
    public Task<bool> Read() => Button.Read();
}