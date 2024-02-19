using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a digital input port implementation for the FT23xx device.
/// </summary>
public sealed class Ft23xxDigitalInputPort : DigitalInputPortBase
{
    private MpsseChannel _device;
    private ResistorMode _resistor;

    internal Ft23xxDigitalInputPort(MpsseChannel device, IPin pin, ResistorMode resistorMode, IDigitalChannelInfo channel)
        : base(pin, channel)
    {
        Resistor = resistorMode;
        _device = device;
    }

    /// <inheritdoc/>
    public override bool State
    {
        get
        {
            // reads all 8 pis at once
            var state = _device.GetGpioState(true);
            // the pin key is the mask
            return (state & (byte)Pin.Key) != 0;
        }
    }

    /// <inheritdoc/>
    public override ResistorMode Resistor
    {
        get => _resistor;
        set
        {
            switch (value)
            {
                case ResistorMode.InternalPullUp:
                case ResistorMode.InternalPullDown:
                    throw new NotSupportedException("Internal resistors are not supported");
                default:
                    _resistor = value;
                    break;
            }
        }
    }
}
