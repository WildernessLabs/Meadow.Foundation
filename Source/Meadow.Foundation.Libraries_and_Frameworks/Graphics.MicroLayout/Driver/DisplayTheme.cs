using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

public class DisplayTheme
{
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public Color? PressedColor { get; set; }
    public Color? HighlightColor { get; set; }
    public Color? ShadowColor { get; set; }
    public Color? TextColor { get; set; }
    public IFont? Font { get; set; }
}
