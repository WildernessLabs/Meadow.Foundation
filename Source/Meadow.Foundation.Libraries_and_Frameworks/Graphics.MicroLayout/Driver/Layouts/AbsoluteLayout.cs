namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout requiring absolute positioning of all child controls
/// </summary>
public class AbsoluteLayout : LayoutBase
{
    /// <summary>
    /// Creates a full-screen DisplayAbsoluteLayout
    /// </summary>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    public AbsoluteLayout(int width, int height)
        : base(0, 0, width, height)
    { }

    /// <summary>
    /// Creates a full-screen DisplayAbsoluteLayout
    /// </summary>
    /// <param name="screen">The DisplayScreen to associate this layout with</param>
    public AbsoluteLayout(DisplayScreen screen)
        : base(0, 0, screen.Width, screen.Height)
    { }

    /// <summary>
    /// Creates a DisplayAbsoluteLayout
    /// </summary>
    /// <param name="left">The layout's left position</param>
    /// <param name="top">The layout's top position</param>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    public AbsoluteLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    { }

    /// <inheritdoc/>
    internal override void PerformLayout()
    {
        //nop
    }

    /// <summary>
    /// Adds a control to the absolute layout
    /// </summary>
    /// <param name="controls">The control(s) to add</param>
    public void Add(params IControl[] controls)
    {
        Controls.Add(controls);
    }
}
