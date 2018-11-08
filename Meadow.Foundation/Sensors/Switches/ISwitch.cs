using System;

namespace Meadow.Foundation.Sensors.Switches
{
    public interface ISwitch
    {
        event EventHandler Changed;

        bool IsOn { get; }
    }
}