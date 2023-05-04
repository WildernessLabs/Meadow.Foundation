using System;

namespace MicroLayout;

public abstract class ClickableDisplayControl : DisplayControl, IClickableDisplayControl
{
    public event EventHandler Clicked;

    private bool _pressed = false;

    public bool Pressed
    {
        get => _pressed;
        set
        {
            if (!Visible) return;
            if (_pressed == value) return;

            _pressed = value;
            if (!Pressed)
            {
                Clicked?.Invoke(this, EventArgs.Empty);
            }
            Invalidate();
        }
    }

    public ClickableDisplayControl()
        : base(0, 0, 10, 10)
    {
    }

    public ClickableDisplayControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }
}
