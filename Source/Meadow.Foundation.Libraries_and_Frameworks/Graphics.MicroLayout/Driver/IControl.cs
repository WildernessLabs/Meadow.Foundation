namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a display control in the user interface.
/// </summary>
public interface IControl
{
    /// <summary>
    /// Gets of sets the Control's Parent, if it has one.  If the Control is unparented (i.e. Parent is null) then it is directly on the DisplayScreen
    /// </summary>
    IControl? Parent { get; set; }

    /// <summary>
    /// Gets or sets the left coordinate of the display control.
    /// </summary>
    int Left { get; set; }

    /// <summary>
    /// Gets or sets the top coordinate of the display control.
    /// </summary>
    int Top { get; set; }

    /// <summary>
    /// Gets or sets whether the control is visible.
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the width of the display control.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the display control.
    /// </summary>
    int Height { get; set; }

    /// <summary>
    /// Gets a value indicating whether the display control is currently invalid and needs to be refreshed.
    /// </summary>
    bool IsInvalid { get; }

    /// <summary>
    /// Refreshes the display control using the specified <see cref="MicroGraphics"/> object.
    /// </summary>
    /// <param name="graphics">The <see cref="MicroGraphics"/> object to use for refreshing the display control.</param>
    void Refresh(MicroGraphics graphics);

    /// <summary>
    /// Marks the display control as invalid and in need of refreshing.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// Checks if the specified coordinates are contained within the display control's area.
    /// </summary>
    /// <param name="x">The x-coordinate to check.</param>
    /// <param name="y">The y-coordinate to check.</param>
    /// <returns><c>true</c> if the coordinates are contained within the display control's area; otherwise, <c>false</c>.</returns>
    public bool Contains(int x, int y)
    {
        if (x < Left) return false;
        if (x > Left + Width) return false;
        if (y < Top) return false;
        if (y > Top + Height) return false;
        return true;
    }
}
