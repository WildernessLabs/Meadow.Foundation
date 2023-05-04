using Meadow.Foundation.Graphics;

namespace MicroLayout;

public enum LayoutDirection
{
    Horizontal,
    Vertical
}

public enum LayoutAlignment
{
    Start,
    Middle,
    End
}

public class StackLayout : IDisplayControl
{
    public LayoutDirection Direction { get; }
    public LayoutAlignment Alignment { get; }
    public int Left { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Top { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Width { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int Height { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public bool IsInvalid => throw new System.NotImplementedException();

    public StackLayout(LayoutDirection direction, LayoutAlignment alignment = LayoutAlignment.Start, params IDisplayControl[] content)
    {
        Direction = direction;
        Alignment = alignment;
    }

    public StackLayout(LayoutDirection direction, LayoutAlignment alignment = LayoutAlignment.Start)
    {
        Direction = direction;
        Alignment = alignment;
    }

    public void ApplyTheme(DisplayTheme theme)
    {
    }

    public void AddControl(IDisplayControl control)
    {
    }

    public void AddControls(params IDisplayControl[] controls)
    {
    }

    public void Refresh(MicroGraphics graphics)
    {
        throw new System.NotImplementedException();
    }

    public void Invalidate()
    {
        throw new System.NotImplementedException();
    }
}
