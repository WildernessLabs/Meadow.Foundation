using Meadow.Hardware;
using System;
using System.Collections.Generic;
using static Meadow.Foundation.Sensors.Hid.Keyboard.Interop;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// An IPin implementation for a Keyboard indicator
/// </summary>
public class KeyboardIndicatorPin : Pin
{
    internal KeyboardIndicatorPin(IPinController controller, string name, Indicators key)
        : base(controller, name, key,
        new List<IChannelInfo>()
        {
            new DigitalChannelInfo(name, inputCapable: false, outputCapable: true, interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
        })
    {
        if (!(controller is Keyboard))
        {
            throw new ArgumentException("KeyboardIndicatorPins are only supported on a Keyboard");
        }
    }

    /// <summary>
    /// Compares this pin to another
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(IPin? other)
    {
        if (other == null) return false;
        return this.Key.Equals(other.Key);
    }
}
