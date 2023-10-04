namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout requiring absolute positioning of all child controls
/// </summary>
public class AbsoluteLayout : Layout
{
    /// <summary>
    /// Creates a full-screen DisplayAbsoluteLayout
    /// </summary>
    /// <param name="screen">The DisplayScreen to associate this layout with</param>
    public AbsoluteLayout(DisplayScreen screen)
        : base(screen, 0, 0, screen.Width, screen.Height)
    {
    }

    /// <summary>
    /// Creates a DisplayAbsoluteLayout
    /// </summary>
    /// <param name="screen">The DisplayScreen to associate this layout with</param>
    /// <param name="left">The layout's left position</param>
    /// <param name="top">The layout's top position</param>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    public AbsoluteLayout(DisplayScreen screen, int left, int top, int width, int height)
        : base(screen, left, top, width, height)
    {
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (Visible && BackgroundColor != null)
        {
            graphics.DrawRectangle(Left, Top, Width, Height, BackgroundColor.Value, true);
        }
    }
}
