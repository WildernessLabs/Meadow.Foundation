using Meadow.Peripherals.Displays;
using SkiaSharp;

namespace Meadow.Foundation.Displays;

internal class SkiaPixelBuffer : IPixelBuffer
{
    public SKBitmap SKBitmap { get; }

    public int Width => SKBitmap.Width;
    public int Height => SKBitmap.Height;

    public ColorMode ColorMode => ColorMode.Format32bppRgba8888;

    public int BitDepth => 32;

    public int ByteCount => SKBitmap.ByteCount;

    public byte[] Buffer => SKBitmap.GetPixelSpan().ToArray();

    public SkiaPixelBuffer(int width, int height)
    {
        SKBitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888));
    }

    public void Clear()
    {
        SKBitmap.Erase(SKColors.Black);
    }

    public void Fill(Color color)
    {
        SKBitmap.Erase(new SKColor(color.R, color.G, color.B));
    }

    public void Fill(int originX, int originY, int width, int height, Color color)
    {
        SKBitmap.Erase(
            new SKColor(color.R, color.G, color.B),
            new SKRectI(originX, originY, originX + width, originY + height));
    }

    public Color GetPixel(int x, int y)
    {
        var px = SKBitmap.GetPixel(x, y);
        return new Color(px.Red, px.Green, px.Blue);
    }

    public void InvertPixel(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void SetPixel(int x, int y, Color color)
    {
        SKBitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B));
    }

    public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
    {
        for (var x = 0; x < buffer.Width; x++)
        {
            for (var y = 0; y < buffer.Height; y++)
            {
                SetPixel(originX + x, originY + y, buffer.GetPixel(x, y));
            }
        }
    }
}