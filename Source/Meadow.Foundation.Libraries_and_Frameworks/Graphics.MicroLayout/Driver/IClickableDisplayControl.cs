using System;

namespace Meadow.Foundation.Displays.UI;

public interface IClickableDisplayControl : IDisplayControl
{
    public event EventHandler Clicked;

    public bool Pressed { get; set; }
}
