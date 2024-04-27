using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Silk.NET.Input;
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
    private IWindow window;
    private SkiaPixelBuffer pixelBuffer = default!;
    private GRGlInterface grglInterface;
    private GRContext context;
    private SKSurface surface;
    private SKCanvas canvas;
    private int virtualWidth;
    private int virtualHeight;
    private float displayScale;

    /// <inheritdoc/>
    public event TouchEventHandler TouchDown = default!;
    /// <inheritdoc/>
    public event TouchEventHandler TouchUp = default!;
    /// <inheritdoc/>
    public event TouchEventHandler TouchClick = default!;
    /// <inheritdoc/>
    public event TouchEventHandler TouchMoved = default!;

    /// <inheritdoc/>
    public RotationType Rotation => RotationType.Normal;

    /// <inheritdoc/>
    public IPixelBuffer PixelBuffer => pixelBuffer;

    /// <inheritdoc/>
    public ColorMode ColorMode => pixelBuffer.ColorMode;

    /// <inheritdoc/>
    public int Width => pixelBuffer.Width;

    /// <inheritdoc/>
    public int Height => pixelBuffer.Height;

    /// <inheritdoc/>
    public ColorMode SupportedColorModes => ColorMode.Format24bppRgb888 | ColorMode.Format16bppRgb565 | ColorMode.Format32bppRgba8888;

    /// <inheritdoc/>
    public bool IsTouched { get; private set; }

    /// <summary>
    /// Create a new SilkDisplay
    /// </summary>
    /// <param name="width">Width of display in pixels</param>
    /// <param name="height">Height of display in pixels</param>
    /// <param name="displayScale"></param>
    public SilkDisplay(int width = 800, int height = 600, float displayScale = 1.0f)
    {
        this.displayScale = displayScale;
        virtualWidth = (int)(width * displayScale);
        virtualHeight = (int)(height * displayScale);
        Initialize(virtualWidth, virtualHeight);
    }

    /// <inheritdoc/>
    public void Resize(int width, int height, float displayScale = 1)
    {
        pixelBuffer = new SkiaPixelBuffer(width, height);

        this.displayScale = displayScale;
        virtualWidth = (int)(width * displayScale);
        virtualHeight = (int)(height * displayScale);
        window.Size = new Vector2D<int>(virtualWidth, virtualHeight);
        CreateOrUpdateDrawingSurface(virtualWidth, virtualHeight);
    }

    private void Initialize(int width, int height)
    {
        pixelBuffer = new SkiaPixelBuffer(width, height);

        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = "Meadow Desktop";
        options.PreferredStencilBufferBits = 8;
        options.PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8);
        options.WindowBorder = WindowBorder.Fixed;
        GlfwWindowing.Use();
        window = Window.Create(options);
        window.Load += OnWindowLoad;
        window.Render += OnWindowRender;
        window.Initialize();

        WindowExtensions.Center(window);

        grglInterface = GRGlInterface.Create(name => window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : 0);
        grglInterface.Validate();
        context = GRContext.CreateGl(grglInterface);
        CreateOrUpdateDrawingSurface(width, height);
    }

    private void CreateOrUpdateDrawingSurface(int width, int height)
    {
        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
        if (canvas != null) canvas.Dispose();
        if (surface != null) surface.Dispose();
        surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        canvas = surface.Canvas;
    }

    private void OnWindowLoad()
    {
        IInputContext input = window.CreateInput();
        var mouse = input.Mice.FirstOrDefault();

        if (mouse != null)
        {
            mouse.MouseDown += (s, e) =>
            {
                IsTouched = true;
                TouchDown?.Invoke(this,
                    TouchPoint.FromScreenData(
                        (int)(mouse.Position.X / displayScale),
                        (int)(mouse.Position.Y / displayScale),
                        0, -1, -1, null));
            };
            mouse.MouseUp += (s, e) =>
            {
                IsTouched = false;
                TouchUp?.Invoke(this,
                    TouchPoint.FromScreenData(
                        (int)(mouse.Position.X / displayScale),
                        (int)(mouse.Position.Y / displayScale),
                        0, -1, -1, null));
            };
            mouse.Click += (s, e, v) =>
            {
                TouchClick?.Invoke(this,
                    TouchPoint.FromScreenData(
                        (int)(v.X / displayScale),
                        (int)(v.Y / displayScale),
                        0, -1, -1, null));
            };
            mouse.MouseMove += (s, e) =>
            {
                TouchMoved?.Invoke(this,
                    TouchPoint.FromScreenData(
                        (int)(mouse.Position.X / displayScale),
                        (int)(mouse.Position.Y / displayScale),
                        0, -1, -1, null));
            };
        }
    }

    private void OnWindowRender(double obj)
    {
        canvas.DrawBitmap(pixelBuffer.SKBitmap,
            SKRect.Create(0, 0, Width, Height),
            SKRect.Create(0, 0, virtualWidth, virtualHeight));

        canvas.Flush();
    }

    /// <summary>
    /// Run the application
    /// </summary>
    public void Run()
    {
        window.Run();
        window.Reset();
        window.Dispose();
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
        pixelBuffer.Clear();
    }

    /// <summary>
    /// Fills the entire display with a given color
    /// </summary>
    /// <param name="fillColor"></param>
    /// <param name="updateDisplay"></param>
    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        pixelBuffer.Fill(fillColor);

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
    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        pixelBuffer.Fill(x, y, width, height, fillColor);
    }

    /// <summary>
    /// Fills a pixel with a given color
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    public void DrawPixel(int x, int y, Color color)
    {
        pixelBuffer.SetPixel(x, y, color);
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
        pixelBuffer.InvertPixel(x, y);
    }

    /// <summary>
    /// Draws to the pixel buffer at a specified location
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="displayBuffer"></param>
    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        pixelBuffer.WriteBuffer(x, y, displayBuffer);
    }
}