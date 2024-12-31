using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

public class VirtualDisplayBase : IVirtualDisplay
{
    public RotationType Rotation { get; }
    /// <inheritdoc/>
    public ColorMode ColorMode { get; }
    /// <inheritdoc/>
    public ColorMode SupportedColorModes { get; }
    /// <inheritdoc/>
    public int Width { get; }
    /// <inheritdoc/>
    public int Height { get; }

    protected VirtualDisplayBase(int width, int height, RotationType rotationType)
    {
        Width = width;
        Height = height;
        Rotation = rotationType;
    }

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => throw new System.NotImplementedException();

    /// <inheritdoc/>
    public void Clear(bool updateDisplay = false)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, Color color)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, bool enabled)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void InvertPixel(int x, int y)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void Show()
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void Show(int left, int top, int right, int bottom)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        throw new System.NotImplementedException();
    }
}
