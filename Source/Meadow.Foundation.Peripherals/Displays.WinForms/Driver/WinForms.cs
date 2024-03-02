using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a WinForms graphics display
/// </summary>
public class WinFormsDisplay : Form, IPixelDisplay, ITouchScreen
{
    /// <inheritdoc/>
    public event TouchEventHandler? TouchDown = default!;
    /// <inheritdoc/>
    public event TouchEventHandler? TouchUp = default!;
    /// <inheritdoc/>
    public event TouchEventHandler? TouchClick = default!;
    /// <inheritdoc/>
    public event TouchEventHandler? TouchMoved = default!;

    private readonly WinFormsPixelBuffer _buffer;

    /// <inheritdoc/>
    public RotationType Rotation => RotationType.Normal;

    /// <inheritdoc/>
    public ColorMode ColorMode => PixelBuffer.ColorMode;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => _buffer;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888;

    /// <inheritdoc/>
    public bool IsTouched { get; private set; }

    /// <summary>
    /// Create a new WinFormsDisplay
    /// </summary>
    /// <param name="width">Width of the display, in pixels</param>
    /// <param name="height">Height of the display, in pixels</param>
    /// <param name="colorMode">The ColorMode of the display</param>
    public WinFormsDisplay(int width = 800, int height = 600, ColorMode colorMode = ColorMode.Format16bppRgb565)
    {
        this.Width = width + (Width - ClientSize.Width);
        this.Height = height + (Height - ClientSize.Height);

        this.Text = "Meadow WinFormsDisplay";
        this.DoubleBuffered = true;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;

        var iconStream = typeof(WinFormsDisplay).Assembly.GetManifestResourceStream("Displays.WinForms.icon.ico");
        this.Icon = new Icon(iconStream!);

        _buffer = new WinFormsPixelBuffer(width, height, colorMode);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _buffer?.Dispose();
        base.Dispose(disposing);
    }

    ///<inheritdoc/>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        TouchDown?.Invoke(this, TouchPoint.FromScreenData(e.X, e.Y, 0, e.X, e.Y, 0));
        IsTouched = true;
        base.OnMouseDown(e);
    }

    ///<inheritdoc/>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        TouchUp?.Invoke(this, TouchPoint.FromScreenData(e.X, e.Y, 0, e.X, e.Y, 0));
        IsTouched = false;
        base.OnMouseUp(e);
    }

    ///<inheritdoc/>
    protected override void OnClick(EventArgs e)
    {
        TouchClick?.Invoke(this, TouchPoint.FromScreenData(-1, -1, 0, -1, -1, 0));
        base.OnClick(e);
    }

    /// <summary>
    /// Performs a full display update
    /// </summary>
    void IPixelDisplay.Show()
    {
        if (this.IsDisposed)
        {
            return;
        }

        try
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
        catch (ObjectDisposedException)
        {
            // NOP - can happen when quitting application
        }
    }

    /// <summary>
    /// Partial screen update
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    void IPixelDisplay.Show(int left, int top, int right, int bottom)
    {
        this.Invalidate(new Rectangle(left, top, right - left, bottom - top), true);
    }

    /// <summary>
    /// Clears the display buffer
    /// </summary>
    /// <param name="updateDisplay"></param>
    void IPixelDisplay.Clear(bool updateDisplay)
    {
        lock (_buffer)
        {
            _buffer.Clear();
        }
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
    void IPixelDisplay.Fill(Color fillColor, bool updateDisplay)
    {
        lock (_buffer)
        {
            _buffer.Fill(fillColor);
        }
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
    void IPixelDisplay.Fill(int x, int y, int width, int height, Color fillColor)
    {
        lock (_buffer)
        {
            _buffer.Fill(x, y, width, height, fillColor);
        }
    }

    /// <summary>
    /// Fills a pixel with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    void IPixelDisplay.DrawPixel(int x, int y, Color color)
    {
        lock (_buffer)
        {
            _buffer.SetPixel(x, y, color);
        }
    }

    /// <summary>
    /// Fills a pixel with either black or white
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="enabled"></param>
    void IPixelDisplay.DrawPixel(int x, int y, bool enabled)
    {
        lock (_buffer)
        {
            _buffer.SetPixel(x, y, enabled ? Color.White : Color.Black);
        }
    }

    /// <summary>
    /// Inverts the pixel at the given location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void IPixelDisplay.InvertPixel(int x, int y)
    {
        lock (_buffer)
        {
            _buffer.InvertPixel(x, y);
        }
    }

    /// <summary>
    /// Draws to the pixel buffer at a specified location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="displayBuffer"></param>
    void IPixelDisplay.WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        lock (_buffer)
        {
            _buffer.WriteBuffer(x, y, displayBuffer);
        }
    }

    ///<inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        lock (_buffer)
        {
            e.Graphics.DrawImage(_buffer.Image, 0, 0);
            base.OnPaint(e);
        }
    }
}