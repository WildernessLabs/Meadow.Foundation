namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout requiring absolute positioning of all child controls
/// </summary>
public class AbsoluteLayout : MicroLayout
{
    /// <summary>
    /// Creates a full-screen DisplayAbsoluteLayout
    /// </summary>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    public AbsoluteLayout(int width, int height)
        : base(0, 0, width, height)
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
        : base(left, top, width, height)
    {
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (IsVisible && BackgroundColor != null)
        {
            graphics.DrawRectangle(
                Left + (Parent?.Left ?? 0),
                Top + (Parent?.Top ?? 0),
                Width,
                Height,
                BackgroundColor.Value,
                true);
        }
    }
}
