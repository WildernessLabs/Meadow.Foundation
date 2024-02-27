using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// An abstraction of a physical screen
/// </summary>
public class DisplayScreen
{
    private readonly IPixelDisplay _display;
    private readonly MicroGraphics _graphics;
    private readonly ITouchScreen? _touchScreen;
    private bool _updateInProgress = false;

    /// <summary>
    /// Gets the collection of controls on the display screen.
    /// </summary>
    public ControlsCollection Controls { get; }

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

    private bool IsInvalid { get; set; }

    internal DisplayTheme? Theme { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayScreen"/> class.
    /// </summary>
    /// <param name="physicalDisplay">The physical display device to use.</param>
    /// <param name="rotation">The rotation type for the display.</param>
    /// <param name="touchScreen">The optional touchscreen interface.</param>
    /// <param name="theme">The display theme to use.</param>
    public DisplayScreen(IPixelDisplay physicalDisplay, RotationType rotation = RotationType.Normal, ITouchScreen? touchScreen = null, DisplayTheme? theme = null)
    {
        Controls = new ControlsCollection(this, null);
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

        if (Resolver.App != null)
        {
            new Thread(DrawLoopThreaded).Start();
        }
        else
        {
            new Thread(DrawLoopOnCaller).Start();
        }
    }

    private void _touchScreen_TouchUp(int x, int y)
    {
        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                if (control is IClickableControl c)
                {
                    if (control.Contains(x, y))
                    {
                        c.Pressed = false;
                    }
                }
            }
        }
    }

    private void _touchScreen_TouchDown(int x, int y)
    {
        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                if (control is IClickableControl c)
                {
                    if (control.Contains(x, y))
                    {
                        c.Pressed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Invalidates the entire screen, causing all controls to redraw
    /// </summary>
    public void Invalidate()
    {
        IsInvalid = true;
    }

    private void RefreshTree(IControl control)
    {
        control.Invalidate();
        control.Refresh(_graphics);

        if (control is Layout l)
        {
            foreach (var c in l.Controls)
            {
                RefreshTree(c);
            }
        }
    }

    /// <summary>
    /// Begins an update process for the display screen, indicating that no drawing should take place until EndUpdate is called
    /// </summary>
    public void BeginUpdate()
    {
        _updateInProgress = true;
    }

    /// <summary>
    /// End an update process for the display screen, indicating that drawing should resume and invalidating the DisplayScreen
    /// </summary>
    public void EndUpdate()
    {
        _updateInProgress = false;
        IsInvalid = true;
    }

    private void DrawLoopOnCaller()
    {
        while (true)
        {
            {
                if (!_updateInProgress && (IsInvalid || Controls.Any(c => c.IsInvalid)))
                {
                    _graphics.Clear(BackgroundColor);

                    lock (Controls.SyncRoot)
                    {
                        foreach (var control in Controls)
                        {
                            if (control != null)
                            {
                                // TODO: micrographics supports invalidating regions - we need to update to invalidate only regions here, too
                                RefreshTree(control);
                            }
                        }
                    }
                    _graphics.Show();
                    IsInvalid = false;
                }
            }

            Thread.Sleep(50);
        }
    }

    private void DrawLoopThreaded()
    {
        while (true)
        {
            Resolver.App.InvokeOnMainThread((_) =>
            {
                lock (Controls.SyncRoot)
                {
                    if (!_updateInProgress && (IsInvalid || Controls.Any(c => c.IsInvalid)))
                    {
                        _graphics.Clear(BackgroundColor);

                        foreach (var control in Controls)
                        {
                            if (control != null)
                            {
                                // TODO: micrographics supports invalidating regions - we need to update to invalidate only regions here, too
                                RefreshTree(control);
                            }
                        }
                        _graphics.Show();
                        IsInvalid = false;
                    }
                }
            });

            Thread.Sleep(50);
        }
    }
}
