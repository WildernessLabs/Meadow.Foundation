using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a display driver for an ASCII console
/// </summary>
public partial class AsciiConsoleDisplay : IPixelDisplay
{
    private readonly CharacterBuffer _buffer;
    private const string _colors = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@"; // 92 "colors"

    /// <inheritdoc/>
    public ColorMode ColorMode => PixelBuffer.ColorMode;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format8bppGray;

    /// <inheritdoc/>
    public int Width => PixelBuffer.Width;

    /// <inheritdoc/>
    public int Height => PixelBuffer.Height;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => _buffer;

    /// <summary>
    /// Creates a new <see cref="AsciiConsoleDisplay"/> instance with the specified width and height
    /// </summary>
    /// <param name="width">The width in pixels</param>
    /// <param name="height">The height in pixels</param>
    public AsciiConsoleDisplay(int width, int height)
    {
        Console.Clear();
        Console.SetCursorPosition(0, 0);

        _buffer = new CharacterBuffer(width, height);
        PixelBuffer.Clear();
    }

    /// <inheritdoc/>
    public void Clear(bool updateDisplay = false)
    {
        PixelBuffer.Clear();
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, Color color)
    {
        PixelBuffer.SetPixel(x, y, color);
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, bool enabled)
    {
        PixelBuffer.SetPixel(x, y, enabled ? Color.White : Color.Black);
    }

    /// <inheritdoc/>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        PixelBuffer.Fill(fillColor);
        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        PixelBuffer.Fill(x, y, width, height, fillColor);
    }

    /// <inheritdoc/>
    public void InvertPixel(int x, int y)
    {
        PixelBuffer.InvertPixel(x, y);
    }

    /// <inheritdoc/>
    public void Show()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(_buffer.GetPixelCharacter(x, y));
            }
        }
    }

    /// <inheritdoc/>
    public void Show(int left, int top, int right, int bottom)
    {
        for (; left < right; left++)
        {
            for (; top < bottom; top++)
            {
                Console.SetCursorPosition(left, top);
                Console.Write(_buffer.GetPixelCharacter(left, top));
            }
        }
    }

    /// <inheritdoc/>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                DrawPixel(x + i, y + j, displayBuffer.GetPixel(i, j));
            }
        }
    }
}