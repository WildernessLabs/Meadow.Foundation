using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using System;

namespace Meadow.Foundation.Displays;

public partial class AsciiDisplay : IGraphicsDisplay
{
    private readonly CharacterBuffer _buffer;
    private const string _colors = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@"; // 92 "colors"

    public ColorMode ColorMode => PixelBuffer.ColorMode;

    public ColorMode SupportedColorModes => ColorMode.Format8bppGray;

    public int Width => PixelBuffer.Width;
    public int Height => PixelBuffer.Height;
    public IPixelBuffer PixelBuffer => _buffer;

    public AsciiDisplay(int width, int height)
    {
        _buffer = new CharacterBuffer(width, height);
        PixelBuffer.Clear();
    }

    public void Clear(bool updateDisplay = false)
    {
        PixelBuffer.Clear();
    }

    public void DrawPixel(int x, int y, Color color)
    {
        PixelBuffer.SetPixel(x, y, color);
    }

    public void DrawPixel(int x, int y, bool enabled)
    {
        PixelBuffer.SetPixel(x, y, enabled ? Color.White : Color.Black);
    }

    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        PixelBuffer.Fill(fillColor);
        if (updateDisplay)
        {
            Show();
        }
    }

    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        PixelBuffer.Fill(x, y, width, height, fillColor);
    }

    public void InvertPixel(int x, int y)
    {
        PixelBuffer.InvertPixel(x, y);
    }

    public void Show()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(_buffer.GetPixelCharcater(x, y));
            }
        }
    }

    public void Show(int left, int top, int right, int bottom)
    {
        for (; left < right; left++)
        {
            for (; top < bottom; top++)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(_buffer.GetPixelCharcater(left, top));
            }
        }
    }

    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
    }
}