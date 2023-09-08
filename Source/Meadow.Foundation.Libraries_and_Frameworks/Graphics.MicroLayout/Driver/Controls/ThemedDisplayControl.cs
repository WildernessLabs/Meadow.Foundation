namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a base class for themed display controls in the user interface.
/// </summary>
public abstract class ThemedDisplayControl : DisplayControl, IThemedDisplayControl
{
    /// <summary>
    /// Applies the specified display theme to the control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    public abstract void ApplyTheme(DisplayTheme theme);

    /// <inheritdoc/>
    public ThemedDisplayControl(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }
}
