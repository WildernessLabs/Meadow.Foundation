using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a WinForms graphics display
/// </summary>
public class WinFormsDisplay : Form, IGraphicsDisplay, ITouchScreen
{
    /// <summary>
    /// Event fired when the display gets a mouse down
    /// </summary>
    public event TouchEventHandler TouchDown = delegate { };
    /// <summary>
    /// Event fired when the display gets a mouse up
    /// </summary>
    public event TouchEventHandler TouchUp = delegate { };
    /// <summary>
    /// Event fired when the display gets a mouse click
    /// </summary>
    public event TouchEventHandler TouchClick = delegate { };

    private WinFormsPixelBuffer _buffer;

    /// <summary>
    /// Current color mode of display
    /// </summary>
    public ColorMode ColorMode => PixelBuffer.ColorMode;
    public IPixelBuffer PixelBuffer => _buffer;

    /// <summary>
    /// The Color mode supported by the display
    /// </summary>
    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888;

    /// <summary>
    /// Create a new WinFormsDisplay
    /// </summary>
    /// <param name="width">Width of the display, in pixles</param>
    /// <param name="height">Height of the display, in pixels</param>
    public WinFormsDisplay(int width = 800, int height = 600, ColorMode colorMode = ColorMode.Format16bppRgb565)
    {
        this.Width = width;
        this.Height = height;

        this.DoubleBuffered = true;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.ControlBox = false;
        _buffer = new WinFormsPixelBuffer(Width, Height, colorMode);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        TouchDown?.Invoke(e.X, e.Y);
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        TouchUp?.Invoke(e.X, e.Y);
        base.OnMouseUp(e);
    }

    protected override void OnClick(EventArgs e)
    {
        TouchClick?.Invoke(-1, -1);
        base.OnClick(e);
    }

    /// <summary>
    /// Performs a full display update
    /// </summary>
    void IGraphicsDisplay.Show()
    {
        this.Invalidate(true);

        if (InvokeRequired)
        {
            Invoke(Update);
        }
        else
        {
            this.Update();
        }
    }

    /// <summary>
    /// Partial screen update
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    void IGraphicsDisplay.Show(int left, int top, int right, int bottom)
    {
        this.Invalidate(new Rectangle(left, top, right - left, bottom - top), true);
    }

    /// <summary>
    /// Clears the display buffer
    /// </summary>
    /// <param name="updateDisplay"></param>
    void IGraphicsDisplay.Clear(bool updateDisplay)
    {
        _buffer.Clear();
        if (updateDisplay)
        {
            this.Show();
        }
    }

    /// <summary>
    /// Fills the entire display with a given color
    /// </summary>
    /// <param name="fillColor"></param>
    /// <param name="updateDisplay"></param>
    void IGraphicsDisplay.Fill(Foundation.Color fillColor, bool updateDisplay)
    {
        _buffer.Fill(fillColor);
        if (updateDisplay)
        {
            this.Show();
        }
    }

    /// <summary>
    /// Fills a region with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fillColor"></param>
    void IGraphicsDisplay.Fill(int x, int y, int width, int height, Foundation.Color fillColor)
    {
        _buffer.Fill(x, y, width, height, fillColor);
    }

    /// <summary>
    /// Fills a pixel with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    void IGraphicsDisplay.DrawPixel(int x, int y, Foundation.Color color)
    {
        _buffer.SetPixel(x, y, color);
    }

    /// <summary>
    /// Fills a pixel with either black or white
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="enabled"></param>
    void IGraphicsDisplay.DrawPixel(int x, int y, bool enabled)
    {
        _buffer.SetPixel(x, y, enabled ? Foundation.Color.White : Foundation.Color.Black);
    }

    /// <summary>
    /// Inverts the pixel at the given location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void IGraphicsDisplay.InvertPixel(int x, int y)
    {
        _buffer.InvertPixel(x, y);
    }

    /// <summary>
    /// Draws to the pixel buffer at a specified location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="displayBuffer"></param>
    void IGraphicsDisplay.WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        _buffer.WriteBuffer(x, y, displayBuffer);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImage(_buffer.Image, 0, 0);
        base.OnPaint(e);
    }
}