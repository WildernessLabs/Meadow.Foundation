using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

public abstract class DisplayControl : IDisplayControl
{
    private int _left;
    private int _top;
    private int _width;
    private int _height;
    private bool _visible = true;

    public bool IsInvalid { get; private set; }
    public object? Context { get; set; }

    public abstract void ApplyTheme(DisplayTheme theme);

    public DisplayControl(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;

        IsInvalid = true;
    }

    protected void SetInvalidatingProperty<T>(ref T field, T value)
    {
        field = value;
        Invalidate();
    }

    // TODO: allow region invalidation?
    public void Invalidate()
    {
        IsInvalid = true;
    }

    public bool Visible
    {
        get => _visible;
        set => SetInvalidatingProperty(ref _visible, value);
    }

    public int Left
    {
        get => _left;
        set => SetInvalidatingProperty(ref _left, value);
    }

    public int Top
    {
        get => _top;
        set => SetInvalidatingProperty(ref _top, value);
    }

    public int Width
    {
        get => _width;
        set => SetInvalidatingProperty(ref _width, value);
    }

    public int Height
    {
        get => _height;
        set => SetInvalidatingProperty(ref _height, value);
    }

    public int Bottom => Top + Height;
    public int Right => Left + Width;

    public void Refresh(MicroGraphics graphics)
    {
        if (IsInvalid)
        {
            if (Visible)
            {
                OnDraw(graphics);
            }
            IsInvalid = false;
        }
    }

    protected abstract void OnDraw(MicroGraphics graphics);
}
