using Meadow.Hardware;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Graphics.MicroLayout;

public class DisplayScreen
{
    private IGraphicsDisplay _display;
    private MicroGraphics _graphics;
    private ITouchScreen? _touchScreen;

    /// <summary>
    /// Gets or sets the collection of controls on the display screen.
    /// </summary>
    public ControlsCollection Controls { get; set; }

    /// <summary>
    /// Gets or sets the background color of the display screen.
    /// </summary>
    public Color BackgroundColor { get; set; }

    /// <summary>
    /// Gets the width of the display screen.
    /// </summary>
    public int Width => _graphics.Width;

    /// <summary>
    /// Gets the height of the display screen.
    /// </summary>
    public int Height => _graphics.Height;

    internal DisplayTheme? Theme { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayScreen"/> class.
    /// </summary>
    /// <param name="physicalDisplay">The physical display device to use.</param>
    /// <param name="rotation">The rotation type for the display.</param>
    /// <param name="touchScreen">The optional touchscreen interface.</param>
    /// <param name="theme">The display theme to use.</param>
    public DisplayScreen(IGraphicsDisplay physicalDisplay, RotationType rotation = RotationType.Normal, ITouchScreen? touchScreen = null, DisplayTheme? theme = null)
    {
        Controls = new ControlsCollection(this);
        Theme = theme;

        _display = physicalDisplay;
        _graphics = new MicroGraphics(_display);

        _graphics.Rotation = rotation;

        _touchScreen = touchScreen;

        if (_touchScreen != null)
        {
            _touchScreen.TouchDown += _touchScreen_TouchDown;
            _touchScreen.TouchUp += _touchScreen_TouchUp;
        }

        if (theme?.Font != null)
        {
            _graphics.CurrentFont = theme.Font;
        }

        BackgroundColor = theme?.BackgroundColor ?? Color.Black;

        new Thread(DrawLoop).Start();
    }

    private void _touchScreen_TouchUp(int x, int y)
    {
        foreach (var control in Controls)
        {
            if (control is IClickableDisplayControl c)
            {
                if (control.Contains(x, y))
                {
                    c.Pressed = false;
                }
            }
        }
    }

    private void _touchScreen_TouchDown(int x, int y)
    {
        foreach (var control in Controls)
        {
            if (control is IClickableDisplayControl c)
            {
                if (control.Contains(x, y))
                {
                    c.Pressed = true;
                }
            }
        }
    }

    private void DrawLoop()
    {
        while (true)
        {
            Resolver.App.InvokeOnMainThread((_) =>
            {
                if (Controls.Count == 0 || Controls.Any(c => c.IsInvalid))
                {
                    _graphics.Clear(BackgroundColor);

                    foreach (var control in Controls)
                    {
                        // until micrographics supports invalidating regions, we have to invalidate everything when one control needs updating
                        control.Invalidate();
                        control.Refresh(_graphics);
                    }

                    _graphics.Show();
                }
            });

            Thread.Sleep(50);
        }
    }
}
