using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Buttons;


/// <summary>
/// Represents a momentary push button with two states that uses interrupts to detect state change
/// </summary>
public class PushButton : PushButtonBase
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
    /// Returns digital input port
    /// </summary>
    protected new IDigitalInterruptPort DigitalIn { get; set; }

    /// <summary>
    /// Creates PushButton with a digital input pin connected on a IIOdevice, specifying if its using an Internal or External PullUp/PullDown resistor.
    /// </summary>
    /// <param name="inputPin">The pin used to create the button port</param>
    /// <param name="resistorMode">The resistor mode</param>
    /// <param name="debounceDuration">The interrupt debounce duration</param>
    public PushButton(IPin inputPin, ResistorMode resistorMode, TimeSpan debounceDuration)
        : this(inputPin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode, debounceDuration, DefaultGlitchDuration))
    {
        ShouldDisposeInput = true;
    }

    /// <summary>
    /// Creates PushButton with a digital input pin connected on a IIOdevice, specifying if its using an Internal or External PullUp/PullDown resistor.
    /// </summary>
    /// <param name="inputPin">The pin used to create the button port</param>
    /// <param name="resistorMode">The resistor mode</param>
    public PushButton(IPin inputPin, ResistorMode resistorMode = ResistorMode.InternalPullUp)
        : this(inputPin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode, DefaultDebounceDuration, DefaultGlitchDuration))
    {
        ShouldDisposeInput = true;
    }

    /// <summary>
    /// Creates PushButton with a pre-configured input port
    /// </summary>
    /// <param name="inputPort"></param>
    public PushButton(IDigitalInterruptPort inputPort)
        : base(inputPort)
    {
        DigitalIn = inputPort;

        LongClickedThreshold = DefaultLongClickThreshold;

        DigitalIn.Changed += DigitalInChanged;
    }

    private void DigitalInChanged(object sender, DigitalPortResult result)
    {
        UpdateEvents(GetNormalizedState(result.New.State));
    }
}