namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a base class for themed display controls in the user interface.
/// </summary>
public abstract class ThemedControl : Control, IThemedControl
{
    /// <summary>
    /// Gets or sets the display theme for the application.
    /// </summary>
    protected DisplayTheme? Theme { get; set; }

    /// <summary>
    /// Applies the specified display theme to the label display control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public virtual void ApplyTheme(DisplayTheme theme)
    {
        Theme = theme;
        Invalidate();
    }

    /// <inheritdoc/>
    public ThemedControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    { }
}