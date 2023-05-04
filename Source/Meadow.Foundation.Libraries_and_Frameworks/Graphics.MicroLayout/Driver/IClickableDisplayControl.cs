using System;

namespace MicroLayout;

public interface IClickableDisplayControl : IDisplayControl
{
    public event EventHandler Clicked;

    public bool Pressed { get; set; }
}
