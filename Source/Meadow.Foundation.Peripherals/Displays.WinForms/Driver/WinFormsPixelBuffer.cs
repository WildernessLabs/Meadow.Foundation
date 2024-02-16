using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a System.Drawing.Bimap-backed pixel buffer
/// </summary>
internal class WinFormsPixelBuffer : IPixelBuffer, IDisposable
{
    private readonly Bitmap _bmp;
    private readonly byte[] _buffer;

    /// <summary>
    /// Gets the buffer width, in pixels
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// Gets the buffer height, in pixels
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the color mode of the buffer
    /// </summary>
    public ColorMode ColorMode { get; }
    /// <summary>
    /// Gets the buffer bit depth
    /// </summary>
    public int BitDepth => 24;
    /// <summary>
    /// Gets the buffer size in bytes
    /// </summary>
    public int ByteCount => _buffer.Length;
    /// <summary>
    /// Gets the buffer backing array
    /// </summary>
    public byte[] Buffer => _buffer;
    /// <summary>
    /// Gets the buffer as a System.Drawing.Bitmap
    /// </summary>
    public Bitmap Image => _bmp;

    /// <summary>
    /// Creates an instance of a WinFormsPixelBuffer
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="colorMode"></param>
    public WinFormsPixelBuffer(int width, int height, ColorMode colorMode = ColorMode.Format24bppRgb888)
    {
        Width = width;
        Height = height;
        ColorMode = colorMode;

        // TODO: use this buffer in a future version for improved perf
        _bmp = new Bitmap(Width, Height);
        var data = _bmp.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        _buffer = new byte[Math.Abs(data.Stride * Height)];
        _bmp.UnlockBits(data);
    }

    /// <summary>
    /// Clears the pixel buffer
    /// </summary>
    public void Clear()
    {
        lock (_bmp)
        {
            using (var g = System.Drawing.Graphics.FromImage(_bmp))
            {
                g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), 0, 0, Width, Height);
            }
        }
    }

    /// <summary>
    /// Fills the entire buffer with a specified color
    /// </summary>
    /// <param name="color"></param>
    public void Fill(Color color)
    {
        lock (_bmp)
        {
            using (var g = System.Drawing.Graphics.FromImage(_bmp))
            {
                g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), 0, 0, Width, Height);
            }
        }
    }

    /// <summary>
    /// Fills a region of the buffer with a specified color
    /// </summary>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    public void Fill(int originX, int originY, int width, int height, Color color)
    {
        try
        {
            lock (_bmp)
            {
                using (var g = System.Drawing.Graphics.FromImage(_bmp))
                {
                    g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), originX, originY, width, height);
                }
            }
        }
        catch (ArgumentException)
        {
            // NOP - can happen on app shutdown
        }
        catch (InvalidOperationException)
        {
            // NOP - can happen on app shutdown
        }
    }

    /// <summary>
    /// Gets the color of a single pixel
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Color GetPixel(int x, int y)
    {
        lock (_bmp)
        {
            var p = _bmp.GetPixel(x, y);
            return Color.FromRgba(p.R, p.G, p.B, p.A);
        }
    }

    /// <summary>
    /// Inverts the color of a single pixel
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InvertPixel(int x, int y)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets the color of a single pixel
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public void SetPixel(int x, int y, Color color)
    {
        // TODO: use lockbits and set the _buffer instead
        lock (_bmp)
        {
            try
            {
                _bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(color.R, color.G, color.B));
            }
            catch (InvalidOperationException)
            {
                // NOP
            }
        }
    }

    /// <summary>
    /// Copies data to the pixel buffer at a specific location
    /// </summary>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="buffer"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
    {
        lock (_bmp)
        {
            for (var x = 0; x < buffer.Width; x++)
            {
                for (var y = 0; y < buffer.Height; y++)
                {
                    SetPixel(x + originX, y + originY, buffer.GetPixel(x, y));
                }
            }
        }
    }

    public void Dispose()
    {
        _bmp?.Dispose();
    }
}
