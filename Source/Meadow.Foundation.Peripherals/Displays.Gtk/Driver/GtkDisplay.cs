using Cairo;
using Gtk;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using Meadow.Hardware;
using System;
using System.Buffers.Binary;
using System.Threading;

namespace Meadow.Foundation.Displays;

public class GtkDisplay : IGraphicsDisplay, ITouchScreen
{
    public event Hardware.TouchEventHandler TouchDown;
    public event Hardware.TouchEventHandler TouchUp;
    public event Hardware.TouchEventHandler TouchClick;

    private Window _window;
    private IPixelBuffer _pixelBuffer;
    private Cairo.Format _format;
    private int _stride;
    private Action<byte[]>? _bufferConverter = null;
    private bool _leftButtonState = false;

    private EventWaitHandle ShowComplete { get; } = new EventWaitHandle(true, EventResetMode.ManualReset);
    public IPixelBuffer PixelBuffer => _pixelBuffer;
    public ColorMode ColorMode => _pixelBuffer.ColorMode;

    public int Width => _window.Window.Width;
    public int Height => _window.Window.Height;

    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888 | ColorMode.Format16bppRgb565 | ColorMode.Format32bppRgba8888;

    static GtkDisplay()
    {
        Application.Init();
    }

    public GtkDisplay(ColorMode mode = ColorMode.Format24bppRgb888)
    {
        Initialize(800, 600, mode); // TODO: query screen size and caps
    }

    public GtkDisplay(int width, int height, ColorMode mode = ColorMode.Format24bppRgb888)
    {
        Initialize(width, height, mode);
    }

    private void Initialize(int width, int height, ColorMode mode)
    {
        _window = new Window(WindowType.Popup);
        _window.WidgetEvent += _window_WidgetEvent;

        switch (mode)
        {
            case ColorMode.Format24bppRgb888:
                _pixelBuffer = new BufferRgb888(width, height);
                _format = Cairo.Format.Rgb24;
                break;
            case ColorMode.Format16bppRgb565:
                _pixelBuffer = new BufferRgb565(width, height);
                _format = Cairo.Format.Rgb16565;
                _bufferConverter = ConvertRGBBufferToBGRBuffer24;
                break;
            case ColorMode.Format32bppRgba8888:
                _pixelBuffer = new BufferRgb8888(width, height);
                _format = Cairo.Format.Argb32;
                break;
            default:
                throw new Exception($"Mode {mode} not supported");
        }
        _stride = GetStrideForBitDepth(width, _pixelBuffer.BitDepth);

        _window.WindowPosition = WindowPosition.Center;
        _window.DefaultSize = new Gdk.Size(width, height);
        _window.ShowAll();
        _window.Drawn += OnWindowDrawn;
    }

    private void RaiseTouchDown(double x, double y)
    {
        _leftButtonState = true;
        TouchDown?.Invoke((int)x, (int)y);
    }

    private void RaiseTouchUp(double x, double y)
    {
        TouchUp?.Invoke((int)x, (int)y);
        if (_leftButtonState)
        {
            TouchClick?.Invoke((int)x, (int)y);
        }
        _leftButtonState = false;

    }

    private void _window_WidgetEvent(object o, WidgetEventArgs args)
    {
        if (args.Event is Gdk.EventButton b)
        {
            switch (b.Button)
            {
                case 1: // left
                    switch (b.State)
                    {
                        case Gdk.ModifierType.None:
                            RaiseTouchDown(b.X, b.Y);
                            break;
                        case Gdk.ModifierType.Button1Mask:
                            RaiseTouchUp(b.X, b.Y);
                            break;
                    }
                    break;
            }
        }
    }

    public void Run()
    {
        Application.Run();
    }

    private void OnWindowDrawn(object o, DrawnArgs args)
    {
        _bufferConverter?.Invoke(_pixelBuffer.Buffer);

        using (var surface = new ImageSurface(_pixelBuffer.Buffer, _format, Width, Height, _stride))
        {
            args.Cr.SetSource(surface);
            args.Cr.Paint();

            if (args.Cr.GetTarget() is IDisposable d) d.Dispose();
            if (args.Cr is IDisposable cd) cd.Dispose();
            args.RetVal = true;
        }
        ShowComplete.Set();
    }

    public void Show()
    {
        _window.QueueDraw();
        ShowComplete.Reset();
    }

    public void Show(int left, int top, int right, int bottom)
    {
        _window.QueueDrawArea(left, top, right - left, bottom - top);
        ShowComplete.Reset();
    }

    public void Clear(bool updateDisplay = false)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.Clear();
    }

    public void Fill(Foundation.Color fillColor, bool updateDisplay = false)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.Fill(fillColor);
    }

    public void Fill(int x, int y, int width, int height, Foundation.Color fillColor)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.Fill(x, y, width, height, fillColor);
    }

    public void DrawPixel(int x, int y, Foundation.Color color)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.SetPixel(x, y, color);
    }

    public void DrawPixel(int x, int y, bool colored)
    {
        ShowComplete.WaitOne();
        DrawPixel(x, y, colored ? Foundation.Color.White : Foundation.Color.Black);
    }

    public void InvertPixel(int x, int y)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.InvertPixel(x, y);
    }

    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        ShowComplete.WaitOne();
        _pixelBuffer.WriteBuffer(x, y, displayBuffer);
    }


    // This attempts to copy the cairo method for getting stride length
    // as defined here:
    // https://github.com/freedesktop/cairo/blob/a934fa66dba2b880723f4e5c3fdea92cbe0207e7/src/cairoint.h#L1553
    // #define CAIRO_STRIDE_FOR_WIDTH_BPP(w,bpp) \
    // ((((bpp)*(w)+7)/8 + CAIRO_STRIDE_ALIGNMENT-1) & -CAIRO_STRIDE_ALIGNMENT)
    private int GetStrideForBitDepth(int width, int bpp)
    {
        var cairo_stride_alignment = sizeof(UInt32);
        var stride = ((((bpp) * (width) + 7) / 8 + cairo_stride_alignment - 1) & -cairo_stride_alignment);
        return stride;
    }

    /// <summary>
    /// 24-bit RGB to BGR converter
    /// </summary>
    /// <param name="buffer"></param>
    private void ConvertRGBBufferToBGRBuffer24(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length; i += 2)
        {
            // convert two bytes into a short
            ushort pixel = (ushort)(buffer[i] << 8 | buffer[i + 1]);

            // reverse the endianness because that's what cairo expects
            BinaryPrimitives.ReverseEndianness(pixel);

            // separate back into two bytes
            buffer[i] = (byte)pixel;
            buffer[i + 1] = (byte)(pixel >> 8);
        }
    }
}
