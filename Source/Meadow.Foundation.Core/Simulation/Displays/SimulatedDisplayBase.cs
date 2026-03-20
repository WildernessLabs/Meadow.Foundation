using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Simulated displayRenderer base class, provides a virtual displayRenderer that renders to a real displayRenderer
/// </summary>
public class SimulatedDisplayBase : IVirtualPixelDisplay
{
    /// <summary>
    /// Is the display rotated (swapped width and height)
    /// </summary>
    public bool IsRotated { get; }

    /// <inheritdoc/>
    public ColorMode ColorMode { get; }

    /// <inheritdoc/>
    public virtual ColorMode SupportedColorModes { get; protected set; }

    /// <inheritdoc/>
    public virtual IPixelDisplay Renderer => displayRenderer;

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <summary>
    /// The enabled color for the displayRenderer
    /// </summary>
    protected virtual Color EnabledColor => Color.White;

    /// <summary>
    /// The disabled color for the displayRenderer
    /// </summary>
    protected virtual Color DisabledColor => Color.Black;

    /// <summary>
    /// The pixel buffer for the displayRenderer
    /// </summary>  
    protected IPixelBuffer pixelBufferDisplay = default!;

    /// <summary>
    /// The bit-per-pixel accurate simulated display buffer
    /// </summary>  
    protected IPixelBuffer pixelBufferSimulated = default!;

    /// <summary>
    /// The real displayRenderer that renders that virtual displayRenderer is rendered on
    /// </summary>
    protected IPixelDisplay displayRenderer;

    /// <summary>
    /// The base constructor for a virtual displayRenderer
    /// </summary>
    /// <param name="displayRenderer"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="rotate"></param>
    /// <param name="colorMode"></param>
    protected SimulatedDisplayBase(
        IResizablePixelDisplay displayRenderer,
        int width, int height,
        bool rotate,
        ColorMode colorMode)
    {
        IsRotated = rotate;
        ColorMode = colorMode;

        Width = IsRotated ? height : width;
        Height = IsRotated ? width : height;

        this.displayRenderer = displayRenderer;
        displayRenderer.Resize(Width, Height, displayRenderer.DisplayScale);

        if (!SupportedColorModes.HasFlag(colorMode))
        {
            Resolver.Log.Warn($"Color mode {colorMode} is not supported by the physical display.");
        }

        pixelBufferSimulated = CreateBuffer(Width, Height, colorMode);

        var realBufType = displayRenderer.PixelBuffer?.GetType();
        if (realBufType is null)
        {
            throw new InvalidOperationException("displayRenderer.PixelBuffer is null; cannot mirror buffer type.");
        }

        var ctor = realBufType.GetConstructor([typeof(int), typeof(int)]);
        if (ctor is null)
        {
            pixelBufferDisplay = CreateBuffer(Width, Height, ColorMode.Format24bppRgb888);
        }
        else
        {
            pixelBufferDisplay = (IPixelBuffer)ctor.Invoke(new object[] { Width, Height });
        }
    }

    IPixelBuffer CreateBuffer(int width, int height, ColorMode colorMode)
    {
        return colorMode switch
        {
            ColorMode.Format1bpp => new Buffer1bpp(width, height),
            ColorMode.Format2bppIndexed => new BufferIndexed2(width, height),
            ColorMode.Format4bppGray => new BufferGray4(width, height),
            ColorMode.Format4bppIndexed => new BufferIndexed4(width, height),
            ColorMode.Format8bppGray => new BufferGray8(width, height),
            ColorMode.Format8bppRgb332 => new BufferRgb332(width, height),
            ColorMode.Format12bppRgb444 => new BufferRgb444(width, height),
            ColorMode.Format16bppRgb565 => new BufferRgb565(width, height),
            ColorMode.Format18bppRgb666 => new BufferRgb666(width, height),
            ColorMode.Format24bppRgb888 => new BufferRgb888(width, height),
            _ => throw new System.NotSupportedException($"Color mode {colorMode} is not supported."),
        };
    }

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => pixelBufferDisplay;

    /// <inheritdoc/>
    public void Clear(bool updateDisplay = false)
    {
        pixelBufferDisplay.Clear();
        pixelBufferSimulated.Clear();

        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, Color color)
    {
        pixelBufferSimulated.SetPixel(x, y, color);
        pixelBufferDisplay.SetPixel(x, y, pixelBufferSimulated.GetPixel(x, y));
    }

    /// <inheritdoc/>
    public void DrawPixel(int x, int y, bool enabled)
    {
        pixelBufferSimulated.SetPixel(x, y, enabled ? EnabledColor : DisabledColor);
        pixelBufferDisplay.SetPixel(x, y, enabled ? EnabledColor : DisabledColor);
    }

    /// <inheritdoc/>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        pixelBufferSimulated.Fill(fillColor);
        pixelBufferDisplay.Fill(pixelBufferSimulated.GetPixel(0, 0));

        if (updateDisplay)
        {
            Show();
        }
    }

    /// <inheritdoc/>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        pixelBufferSimulated.Fill(x, y, width, height, fillColor);
        pixelBufferDisplay.Fill(x, y, width, height, pixelBufferSimulated.GetPixel(x, y));
    }

    /// <inheritdoc/>
    public void InvertPixel(int x, int y)
    {
        pixelBufferSimulated.InvertPixel(x, y);
        pixelBufferDisplay.SetPixel(x, y, pixelBufferSimulated.GetPixel(x, y));
    }

    /// <inheritdoc/>
    public void Show()
    {
        displayRenderer.WriteBuffer(0, 0, pixelBufferDisplay);
        displayRenderer.Show();
    }

    /// <inheritdoc/>
    public void Show(int left, int top, int right, int bottom)
    {
        //ToDo - not in IPixelDisplay interface
        Show();
    }

    /// <inheritdoc/>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        pixelBufferSimulated.WriteBuffer(x, y, displayBuffer);

        for (int i = x; i < x + displayBuffer.Width; i++)
        {
            for (int j = y; j < y + displayBuffer.Height; j++)
            {
                pixelBufferDisplay.SetPixel(i, j, pixelBufferSimulated.GetPixel(i, j));
            }
        }
    }
}