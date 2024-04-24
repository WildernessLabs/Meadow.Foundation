using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;

namespace Meadow.Foundation.Displays;

/// <summary>
/// Represents a pixel display using Silk.NET and OpenGL
/// </summary>
public class SilkDisplay : IResizablePixelDisplay, ITouchScreen
{
    private IWindow _window;
    private SkiaPixelBuffer _pixelBuffer = default!;
    private GRGlInterface _grglInterface;
    private GRContext _context;
    private SKSurface _surface;
    private SKCanvas _canvas;

    /// <inheritdoc/>
    public event Hardware.TouchEventHandler TouchDown = default!;
    /// <inheritdoc/>
    public event Hardware.TouchEventHandler TouchUp = default!;
    /// <inheritdoc/>
    public event Hardware.TouchEventHandler TouchClick = default!;
    /// <inheritdoc/>
    public event Hardware.TouchEventHandler TouchMoved = default!;

    /// <inheritdoc/>
    public RotationType Rotation => RotationType.Normal;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => _pixelBuffer;

    /// <inheritdoc/>
    public ColorMode ColorMode => _pixelBuffer.ColorMode;

    /// <inheritdoc/>
    public int Width => _pixelBuffer.Width;

    /// <inheritdoc/>
    public int Height => _pixelBuffer.Height;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888 | ColorMode.Format16bppRgb565 | ColorMode.Format32bppRgba8888;

    /// <inheritdoc/>
    public bool IsTouched { get; private set; }

    /// <summary>
    /// Create a new SilkDisplay with a default size of 800x600
    /// </summary>
    public SilkDisplay()
    {
        Initialize(800, 600); // TODO: query screen size and caps
    }

    /// <summary>
    /// Create a new SilkDisplay
    /// </summary>
    /// <param name="width">Width of display in pixels</param>
    /// <param name="height">Height of display in pixels</param>
    public SilkDisplay(int width, int height)
    {
        Initialize(width, height);
    }

    /// <inheritdoc/>
    public void Resize(int width, int height, float displayScale = 1)
    {
        throw new NotSupportedException();
    }

    private void Initialize(int width, int height)
    {
        _pixelBuffer = new SkiaPixelBuffer(width, height);

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = "Meadow Desktop";
        options.PreferredStencilBufferBits = 8;
        options.PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8);
        options.WindowBorder = WindowBorder.Fixed;
        GlfwWindowing.Use();
        _window = Window.Create(options);
        _window.Initialize();
        _window.Render += OnWindowRender;
        _grglInterface = GRGlInterface.Create((name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0));
        _grglInterface.Validate();
        _context = GRContext.CreateGl(_grglInterface);
        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        _surface = SKSurface.Create(_context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        _canvas = _surface.Canvas;
    }

    private void OnWindowRender(double obj)
    {
        _canvas.DrawBitmap(_pixelBuffer.SKBitmap, 0, 0);
        _canvas.Flush();
    }

    private void RaiseTouchDown(double x, double y)
    {
    }

    private void RaiseTouchUp(double x, double y)
    {
    }


    /// <summary>
    /// Run the application
    /// </summary>
    public void Run()
    {
        _window.Run();
    }


    /// <summary>
    /// Performs a full display update
    /// </summary>
    public void Show()
    {
    }

    /// <summary>
    /// Partial screen update
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    public void Show(int left, int top, int right, int bottom)
    {
        Show();
    }

    /// <summary>
    /// Clears the display buffer
    /// </summary>
    /// <param name="updateDisplay"></param>
    public void Clear(bool updateDisplay = false)
    {
        _pixelBuffer.Clear();
    }

    /// <summary>
    /// Fills the entire display with a given color
    /// </summary>
    /// <param name="fillColor"></param>
    /// <param name="updateDisplay"></param>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        _pixelBuffer.Fill(fillColor);
        _window.DoRender();
    }

    /// <summary>
    /// Fills a region with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fillColor"></param>
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        _pixelBuffer.Fill(x, y, width, height, fillColor);
    }

    /// <summary>
    /// Fills a pixel with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public void DrawPixel(int x, int y, Color color)
    {
        _pixelBuffer.SetPixel(x, y, color);
    }

    /// <summary>
    /// Fills a pixel with either black or white
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="colored"></param>
    public void DrawPixel(int x, int y, bool colored)
    {
        DrawPixel(x, y, colored ? Color.White : Color.Black);
    }

    /// <summary>
    /// Inverts the pixel at the given location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void InvertPixel(int x, int y)
    {
        _pixelBuffer.InvertPixel(x, y);
    }

    /// <summary>
    /// Draws to the pixel buffer at a specified location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="displayBuffer"></param>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        _pixelBuffer.WriteBuffer(x, y, displayBuffer);
    }
}
