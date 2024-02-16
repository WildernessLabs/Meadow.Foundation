﻿using Meadow.Hardware;
using System;
using System.Collections.Generic;
using static Meadow.Foundation.Sensors.Hid.Keyboard;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// An IPin implementation for a Keyboard input key
/// </summary>
public class KeyboardKeyPin : Pin
{
    /// <summary>
    /// The virtual key code of the key
    /// </summary>
    public new char Key => Convert.ToChar(base.Key);

    internal InteropMac.MacKeyCodes? MacKeyCode { get; }

    internal KeyboardKeyPin(IPinController? controller, string name, char key, InteropMac.MacKeyCodes? macKey)
        : base(controller, name, char.ToUpper(key),
        new List<IChannelInfo>()
        {
            new DigitalChannelInfo(name, interruptCapable: true, pullUpCapable: false, pullDownCapable: false)
        })
    {
        if (controller is not Keyboard)
        {
            throw new ArgumentException("KeyboardKeyPins are only supported on a Keyboard");
        }
        MacKeyCode = macKey;
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
