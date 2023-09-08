namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a display control in the user interface.
/// </summary>
public interface IThemedDisplayControl : IDisplayControl
{
    /// <summary>
    /// Applies the specified display theme to the display control.
    /// </summary>
    /// <param name="theme">The display theme to apply.</param>
    void ApplyTheme(DisplayTheme theme);
}
