using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Meadow.Foundation.Displays;

internal class WinFormsPixelBuffer : IPixelBuffer
{
    private Bitmap _bmp;
    private byte[] _buffer;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public ColorMode ColorMode => ColorMode.Format24bppRgb888;
    public int BitDepth => 24;
    public int ByteCount => _buffer.Length;
    public byte[] Buffer => _buffer;
    public Bitmap Image => _bmp;

    public WinFormsPixelBuffer(int width, int height)
    {
        Width = width;
        Height = height;

        _bmp = new Bitmap(Width, Height);
        var data = _bmp.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        _buffer = new byte[Math.Abs(data.Stride * Height)];
        _bmp.UnlockBits(data);
    }

    public void Clear()
    {
        using (var g = System.Drawing.Graphics.FromImage(_bmp))
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.Black), 0, 0, Width, Height);
        }
    }

    public void Fill(Foundation.Color color)
    {
        using (var g = System.Drawing.Graphics.FromImage(_bmp))
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), 0, 0, Width, Height);
        }
    }

    public void Fill(int originX, int originY, int width, int height, Foundation.Color color)
    {
        using (var g = System.Drawing.Graphics.FromImage(_bmp))
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B)), originX, originX, width, height);
        }
    }

    public Foundation.Color GetPixel(int x, int y)
    {
        var p = _bmp.GetPixel(x, y);
        return Foundation.Color.FromRgba(p.R, p.G, p.B, p.A);
    }

    public void InvertPixel(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void SetPixel(int x, int y, Foundation.Color color)
    {
        // TODO: use lockbits and set the _buffer instead
        _bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(color.R, color.G, color.B));
    }

    public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
    {
        throw new NotImplementedException();
    }
}
