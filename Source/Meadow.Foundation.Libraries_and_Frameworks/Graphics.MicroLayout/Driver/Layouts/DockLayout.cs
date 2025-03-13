using System.Collections.Generic;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls based on a docking position.
/// </summary>
public class DockLayout : MicroLayout
{
    public enum DockPosition
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    public int Padding { get; set; } = 2;

    private readonly Dictionary<IControl, DockPosition> _dockPositions = new();

    public DockLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    public void Add(IControl control, DockPosition position)
    {
        Controls.Add(control);
        _dockPositions[control] = position;

        ArrangeLayout(control, position);
    }

    private void ArrangeLayout(IControl control, DockPosition position)
    {
        switch (position)
        {
            case DockPosition.Top:
                control.Left = base.Width / 2 - control.Width / 2;
                control.Top = Padding;
                break;
            case DockPosition.Bottom:
                control.Left = Width / 2 - control.Width / 2;
                control.Top = Height - Padding - control.Height;
                break;
            case DockPosition.Left:
                control.Left = Padding;
                control.Top = Height / 2 - control.Height / 2;
                break;
            case DockPosition.Right:
                control.Left = Width - control.Width - Padding;
                control.Top = Height / 2 - control.Height / 2;
                break;
            case DockPosition.TopLeft:
                control.Left = Padding;
                control.Top = Padding;
                break;
            case DockPosition.TopRight:
                control.Left = Width - control.Width - Padding;
                control.Top = Padding;
                break;
            case DockPosition.BottomLeft:
                control.Left = Padding;
                control.Top = Height - control.Height - Padding;
                break;
            case DockPosition.BottomRight:
                control.Left = Width - control.Width - Padding;
                control.Top = Height - control.Height - Padding;
                break;
            case DockPosition.Center:
                control.Left = Width / 2 - control.Width / 2;
                control.Top = Height / 2 - control.Height / 2;
                break;
        }
    }

    protected override void OnDraw(MicroGraphics graphics)
    {
        if (!IsVisible || BackgroundColor == null)
            return;

        graphics.DrawRectangle(
            Left + (Parent?.Left ?? 0),
            Top + (Parent?.Top ?? 0),
            Width,
            Height,
            BackgroundColor.Value,
            true);
    }
}