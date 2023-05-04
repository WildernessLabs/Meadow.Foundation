using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System.Linq;
using System.Threading;

namespace MicroLayout;

public class DisplayScreen
{
    public ControlsCollection Controls { get; set; }

    public Color BackgroundColor { get; set; }
    public int Width => _graphics.Width;
    public int Height => _graphics.Height;

    private IGraphicsDisplay _display;
    private MicroGraphics _graphics;
    private ITouchScreen? _touchScreen;

    internal DisplayTheme? Theme { get; }

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
                if (Controls.Any(c => c.IsInvalid))
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
