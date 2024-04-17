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

    /// <inheritdoc/>
    public new int Width => virtualWidth;

    /// <inheritdoc/>
    public new int Height => virtualHeight;

    private readonly WinFormsPixelBuffer buffer;

    /// <inheritdoc/>
    public RotationType Rotation => RotationType.Normal;

    /// <inheritdoc/>
    public ColorMode ColorMode => PixelBuffer.ColorMode;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => buffer;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888;

    /// <inheritdoc/>
    public bool IsTouched { get; private set; }

    private readonly int virtualWidth;
    private readonly int virtualHeight;
    private readonly float displayScale;

    /// <summary>
    /// Create a new WinFormsDisplay
    /// </summary>
    /// <param name="width">Width of the display, in pixels</param>
    /// <param name="height">Height of the display, in pixels</param>
    /// <param name="colorMode">The ColorMode of the display</param>
    /// <param name="displayScale">The scale of the display</param>
    public WinFormsDisplay(int width = 800, int height = 600, ColorMode colorMode = ColorMode.Format16bppRgb565, float displayScale = 1.0f)
    {
        this.displayScale = displayScale;

        ClientSize = new Size((int)(width * displayScale), (int)(height * displayScale));

        Text = "Meadow WinFormsDisplay";
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;

        var iconStream = typeof(WinFormsDisplay).Assembly.GetManifestResourceStream("Displays.WinForms.icon.ico");
        Icon = new Icon(iconStream!);

        buffer = new WinFormsPixelBuffer(virtualWidth = width, virtualHeight = height, colorMode);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        buffer?.Dispose();
        base.Dispose(disposing);
    }

    ///<inheritdoc/>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        int virtualX = (int)(e.X / displayScale);
        int virtualY = (int)(e.Y / displayScale);
        TouchDown?.Invoke(this, TouchPoint.FromScreenData(virtualX, virtualY, 0, virtualX, virtualY, 0));
        IsTouched = true;
        base.OnMouseDown(e);
    }

    ///<inheritdoc/>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        int virtualX = (int)(e.X / displayScale);
        int virtualY = (int)(e.Y / displayScale);
        TouchUp?.Invoke(this, TouchPoint.FromScreenData(virtualX, virtualY, 0, virtualX, virtualY, 0));
        IsTouched = false;
        base.OnMouseUp(e);
    }

    ///<inheritdoc/>
    protected override void OnClick(EventArgs e)
    {
        Point clientPoint = PointToClient(Cursor.Position);

        int virtualX = (int)(clientPoint.X / displayScale);
        int virtualY = (int)(clientPoint.Y / displayScale);

        TouchClick?.Invoke(this, TouchPoint.FromScreenData(virtualX, virtualY, 0, virtualX, virtualY, 0));
        base.OnClick(e);
    }

    /// <summary>
    /// Performs a full display update
    /// </summary>
    void IPixelDisplay.Show()
    {
        if (IsDisposed)
        {
            return;
        }

        try
        {
            Invalidate(true);
            if (InvokeRequired)
            {
                Invoke(Update);
            }
            else
            {
                Update();
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
        var scaledLeft = (int)(left * displayScale);
        var scaledTop = (int)(top * displayScale);
        var scaledRight = (int)((right - left) * displayScale);
        var scaledBottom = (int)((bottom - top) * displayScale);

        Invalidate(new Rectangle(scaledLeft, scaledTop, scaledRight, scaledBottom), true);
    }

    /// <summary>
    /// Clears the display buffer
    /// </summary>
    /// <param name="updateDisplay"></param>
    void IPixelDisplay.Clear(bool updateDisplay)
    {
        lock (buffer)
        {
            buffer.Clear();
        }
        if (updateDisplay)
        {
            Show();
        }
    }

    /// <summary>
    /// Fills the entire display with a given color
    /// </summary>
    /// <param name="fillColor"></param>
    /// <param name="updateDisplay"></param>
    void IPixelDisplay.Fill(Color fillColor, bool updateDisplay)
    {
        lock (buffer)
        {
            buffer.Fill(fillColor);
        }
        if (updateDisplay)
        {
            Show();
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
        lock (buffer)
        {
            buffer.Fill(x, y, width, height, fillColor);
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
        lock (buffer)
        {
            buffer.SetPixel(x, y, color);
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
        lock (buffer)
        {
            buffer.SetPixel(x, y, enabled ? Color.White : Color.Black);
        }
    }

    /// <summary>
    /// Inverts the pixel at the given location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void IPixelDisplay.InvertPixel(int x, int y)
    {
        lock (buffer)
        {
            buffer.InvertPixel(x, y);
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
        lock (buffer)
        {
            buffer.WriteBuffer(x, y, displayBuffer);
        }
    }

    ///<inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        lock (buffer)
        {
            // Apply scaling
            e.Graphics.ScaleTransform(displayScale, displayScale);

            // Draw the scaled image
            e.Graphics.DrawImage(buffer.Image, 0, 0, virtualWidth, virtualHeight);
            base.OnPaint(e);
        }
    }
}