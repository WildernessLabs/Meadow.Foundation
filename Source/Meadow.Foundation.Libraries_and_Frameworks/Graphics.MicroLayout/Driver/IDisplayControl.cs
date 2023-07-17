using Meadow.Foundation.Graphics;

namespace Meadow.Foundation.Displays.UI;

public interface IDisplayControl
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    bool IsInvalid { get; }
    public void Refresh(MicroGraphics graphics);
    void Invalidate();

    public bool Contains(int x, int y)
    {
        if (x < Left) return false;
        if (x > Left + Width) return false;
        if (y < Top) return false;
        if (y > Top + Height) return false;
        return true;
    }

    void ApplyTheme(DisplayTheme theme);
}
