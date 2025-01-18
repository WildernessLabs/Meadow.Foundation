using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// Represents a 4 switch digital joystick / directional pad (D-pad) with a center push button
/// </summary>
public class DigitalPushButtonJoystick : DigitalJoystick, IDigitalPushButtonJoystick
{
    /// <inheritdoc/>
    public TimeSpan LongClickedThreshold { get => _centerButton.LongClickedThreshold; set => _centerButton.LongClickedThreshold = value; }

    /// <inheritdoc/>
    public bool State => _centerButton.State;

    /// <inheritdoc/>
    public event EventHandler? PressStarted;
    /// <inheritdoc/>
    public event EventHandler? PressEnded;
    /// <inheritdoc/>
    public event EventHandler? Clicked;
    /// <inheritdoc/>
    public event EventHandler? LongClicked;

    private readonly PushButton _centerButton;

    /// <summary>
    /// Create a new DigitalJoystick object
    /// </summary>
    /// <param name="pinUp">The pin connected to the up switch</param>
    /// <param name="pinDown">The pin connected to the down switch</param>
    /// <param name="pinLeft">The pin connected to the left switch</param>
    /// <param name="pinRight">The pin connected to the right switch</param>
    /// <param name="pinCenter">The pin connected to the center switch</param>
    /// <param name="resistorMode">The resistor mode for all pins</param>
    public DigitalPushButtonJoystick(IPin pinUp, IPin pinDown, IPin pinLeft, IPin pinRight, IPin pinCenter, ResistorMode resistorMode)
        : base(pinUp, pinDown, pinLeft, pinRight, resistorMode)
    {
        var centerPort = pinCenter.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, resistorMode);
        _centerButton = new PushButton(centerPort);

        _centerButton.PressStarted += (s, e) => { PressStarted?.Invoke(this, e); };
        _centerButton.PressEnded += (s, e) => { PressEnded?.Invoke(this, e); };
        _centerButton.Clicked += (s, e) => { Clicked?.Invoke(this, e); };
        _centerButton.LongClicked += (s, e) => { LongClicked?.Invoke(this, e); };
    }

    /// <inheritdoc/>
    public Task<bool> Read()
    {
        return _centerButton.Read();
    }
}
