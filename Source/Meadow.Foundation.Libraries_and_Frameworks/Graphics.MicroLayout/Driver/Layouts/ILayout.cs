namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// 
/// </summary>
public interface ILayout : IThemedControl, IControlContainer
{
    /// <summary>
    /// Gets or sets the background color of the Layout.
    /// </summary>
    Color? BackgroundColor { get; set; }
}
