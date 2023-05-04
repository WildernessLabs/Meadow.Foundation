using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroLayout;

public class DisplayTheme
{
    public Color? BackgroundColor { get; set; }
    public Color? ForeColor { get; set; }
    public Color? PressedColor { get; set; }
    public Color? HighlightColor { get; set; }
    public Color? ShadowColor { get; set; }
    public Color? TextColor { get; set; }
    public IFont? Font { get; set; }
}
