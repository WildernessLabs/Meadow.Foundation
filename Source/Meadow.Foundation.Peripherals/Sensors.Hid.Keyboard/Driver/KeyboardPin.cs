using Meadow.Hardware;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Hid;

public class KeyboardPin : Pin
{
    public new char Key => Convert.ToChar(base.Key);

    internal KeyboardPin(IPinController controller, string name, char key)
        : base(controller, name, char.ToUpper(key),
        new List<IChannelInfo>()
        {
            new DigitalChannelInfo(name, interruptCapable: true, pullUpCapable: false, pullDownCapable: false)
        })
    {
        if (!(controller is Keyboard))
        {
            throw new ArgumentException("KeyboardPins are only supported on a Keyboard");
        }
    }

    public override bool Equals(IPin? other)
    {
        if (other == null) return false;
        return this.Key.Equals(other.Key);
    }
}
