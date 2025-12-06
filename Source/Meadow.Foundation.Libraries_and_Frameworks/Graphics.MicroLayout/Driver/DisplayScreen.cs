using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// An abstraction of a physical screen
/// </summary>
public class DisplayScreen : IControlContainer
{
    private readonly IPixelDisplay _display;
    private readonly MicroGraphics _graphics;
    private bool _updateInProgress = false;
    private Color _backgroundColor;

    /// <summary>
    /// Occurs when the bounds of the object change.
    /// </summary>
    /// <remarks>This event is raised whenever the size or position of the object's bounds is modified.
    /// Subscribers can use this event to respond to changes in the object's layout or dimensions.</remarks>
    public event EventHandler? BoundsChanged;

    /// <summary>
    /// Gets the Touchscreen associated with the display screen
    /// </summary>
    public ITouchScreen? TouchScreen { get; }

    /// <summary>
    /// Gets the collection of controls on the display screen.
    /// </summary>
    public ControlsCollection Controls { get; }

    /// <summary>
    /// Gets the width of the display screen.
    /// </summary>
    public int Width { get => _graphics.Width; set { } }

    /// <summary>
    /// Gets the height of the display screen.
    /// </summary>
    public int Height { get => _graphics.Height; set { } }

    /// <inheritdoc/>
    public bool IsInvalid { get; private set; }

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
        Controls = new ControlsCollection(this);
        Theme = theme;

        _display = physicalDisplay;
        _graphics = new MicroGraphics(_display);

        _graphics.Rotation = rotation;

        TouchScreen = touchScreen;

        if (TouchScreen != null)
        {
            TouchScreen.TouchDown += OnTouchDown;
            TouchScreen.TouchUp += OnTouchUp;
        }

        if (theme?.Font != null)
        {
            _graphics.CurrentFont = theme.Font;
        }

        _backgroundColor = theme?.BackgroundColor ?? _display.DisabledColor;

        if (Resolver.App != null)
        {
            new Thread(DrawLoopThreaded).Start();
        }
        else
        {
            new Thread(DrawLoopOnCaller).Start();
        }
    }

    /// <summary>
    /// Gets or sets the background color of the display screen.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (value == BackgroundColor) return;
            _backgroundColor = value;
            Invalidate();
        }
    }

    private void OnTouchUp(ITouchScreen source, TouchPoint point)
    {
        bool LookForUnclick(ControlsCollection controls)
        {
            foreach (var control in controls)
            {
                if (control is IClickableControl c)
                {
                    if (control.IsVisible && control.Contains(point.ScreenX, point.ScreenY))
                    {
                        c.Pressed = false;
                        return true;
                    }
                }
                else if (control is IControlContainer container)
                {
                    if (LookForUnclick(container.Controls))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        lock (Controls.SyncRoot)
        {
            LookForUnclick(Controls);
        }
    }

    private void OnTouchDown(ITouchScreen source, TouchPoint point)
    {
        bool LookForClick(ControlsCollection controls)
        {
            foreach (var control in controls)
            {
                if (control is IClickableControl c)
                {
                    if (control.IsVisible && control.Contains(point.ScreenX, point.ScreenY))
                    {
                        c.Pressed = true;
                        return true;
                    }
                }
                else if (control is IControlContainer container)
                {
                    if (LookForClick(container.Controls))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        lock (Controls.SyncRoot)
        {
            LookForClick(Controls);
        }
    }

    /// <summary>
    /// Invalidates the entire screen, causing all controls to redraw
    /// </summary>
    public void Invalidate()
    {
        IsInvalid = true;
    }

    private void Refresh(IControl control)
    {
        control.Invalidate();
        control.Refresh(_graphics);
    }

    /// <summary>
    /// Collects all invalid controls recursively, including nested controls in containers
    /// </summary>
    private void CollectInvalidControls(ControlsCollection controls, List<IControl> invalidControls)
    {
        foreach (var control in controls)
        {
            if (control.IsInvalid && control.IsVisible)
            {
                invalidControls.Add(control);
            }

            // Recursively collect invalid controls from nested containers
            if (control is IControlContainer container)
            {
                CollectInvalidControls(container.Controls, invalidControls);
            }
        }
    }

    /// <summary>
    /// Calculates the bounding rectangle that encompasses all invalid controls
    /// </summary>
    private bool TryGetDirtyRegion(out int left, out int top, out int right, out int bottom)
    {
        var invalidControls = new List<IControl>();
        CollectInvalidControls(Controls, invalidControls);

        if (invalidControls.Count == 0)
        {
            left = top = right = bottom = 0;
            return false;
        }

        left = invalidControls.Min(c => c.ScreenLeft);
        top = invalidControls.Min(c => c.ScreenTop);
        right = invalidControls.Max(c => c.ScreenRight);
        bottom = invalidControls.Max(c => c.ScreenBottom);

        // Clamp to screen bounds
        left = Math.Max(0, left);
        top = Math.Max(0, top);
        right = Math.Min(Width, right);
        bottom = Math.Min(Height, bottom);

        return true;
    }

    /// <summary>
    /// Checks if a control's bounds intersect with the given region
    /// </summary>
    private bool IntersectsRegion(IControl control, int left, int top, int right, int bottom)
    {
        return control.ScreenLeft < right && control.ScreenRight > left &&
               control.ScreenTop < bottom && control.ScreenBottom > top;
    }

    /// <summary>
    /// Collects all controls that intersect with the dirty region (for redrawing overlapping controls)
    /// </summary>
    private void CollectControlsInRegion(ControlsCollection controls, int left, int top, int right, int bottom, List<IControl> controlsToRedraw)
    {
        foreach (var control in controls)
        {
            if (control.IsVisible && IntersectsRegion(control, left, top, right, bottom))
            {
                controlsToRedraw.Add(control);
            }

            // Recursively collect controls from nested containers
            if (control is IControlContainer container)
            {
                CollectControlsInRegion(container.Controls, left, top, right, bottom, controlsToRedraw);
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

    private void DrawLoopProc()
    {
        lock (Controls.SyncRoot)
        {
            if (!_updateInProgress && (IsInvalid || Controls.Any(c => c.IsInvalid)))
            {
                if (IsInvalid)
                {
                    // Full screen invalidation - clear and redraw everything
                    _graphics.Clear(BackgroundColor);

                    foreach (var control in Controls)
                    {
                        if (control != null)
                        {
                            Refresh(control);
                        }
                    }

                    try
                    {
                        _graphics.Show();
                    }
                    catch (Exception ex)
                    {
                        // it's possible to have a callee error (e.g. an I2C bus problem)
                        // we'll report it and continue running
                        Resolver.Log.Warn($"MicroGraphics.Show error while drawing screen: {ex.Message}");
                    }
                }
                else if (TryGetDirtyRegion(out int left, out int top, out int right, out int bottom))
                {
                    // Partial screen invalidation - only update dirty region
                    int width = right - left;
                    int height = bottom - top;

                    // Clear only the dirty region
                    _graphics.DrawRectangle(left, top, width, height, BackgroundColor, true);

                    // Collect all controls that intersect with the dirty region (including overlapping ones)
                    var controlsToRedraw = new List<IControl>();
                    CollectControlsInRegion(Controls, left, top, right, bottom, controlsToRedraw);

                    // Redraw all controls in the dirty region
                    foreach (var control in controlsToRedraw)
                    {
                        Refresh(control);
                    }

                    try
                    {
                        // Update only the dirty region on the display
                        _graphics.Show(left, top, right, bottom);
                    }
                    catch (Exception ex)
                    {
                        // it's possible to have a callee error (e.g. an I2C bus problem)
                        // we'll report it and continue running
                        Resolver.Log.Warn($"MicroGraphics.Show error while drawing screen: {ex.Message}");
                    }
                }

                IsInvalid = false;
            }
        }
    }

    private void DrawLoopOnCaller()
    { // this loop is used by platforms where drawing can happen on any thread (e.g. meadow, or Linux with a SPI display)
        while (true)
        {
            if (!_updateInProgress && (IsInvalid || Controls.Any(c => c.IsInvalid)))
            {
                DrawLoopProc();
            }

            Thread.Sleep(50);
        }
    }

    private void DrawLoopThreaded()
    { // this loop is used by desktop platforms where drawing must happen on a UI thread
        while (true)
        {
            Resolver.App.InvokeOnMainThread((_) =>
            {
                DrawLoopProc();
            });

            Thread.Sleep(50);
        }
    }

    /// <inheritdoc/>
    public void Refresh(MicroGraphics graphics)
    {
        this.Invalidate();
    }

    /// <inheritdoc/>
    public int Left { get => 0; set { } }
    /// <inheritdoc/>
    public int Top { get => 0; set { } }
    /// <inheritdoc/>
    public bool IsVisible { get => true; set { } }
    /// <inheritdoc/>
    public IControl? Parent { get => null; set { } }
}