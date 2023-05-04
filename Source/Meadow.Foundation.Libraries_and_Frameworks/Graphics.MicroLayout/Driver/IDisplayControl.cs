using Meadow.Foundation.Graphics;

namespace MicroLayout;

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
        if (x < this.Left) return false;
        if (x > this.Left + this.Width) return false;
        if (y < this.Top) return false;
        if (y > this.Top + this.Height) return false;
        return true;
    }

    void ApplyTheme(DisplayTheme theme);
}
