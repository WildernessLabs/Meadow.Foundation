using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
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
    private bool _anyControlInvalid = false;
    private readonly List<IControl> _reusableControlList = new List<IControl>();
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

    /// <summary>
    /// Internal method called by controls to notify the screen that a control has been invalidated
    /// </summary>
    internal void NotifyControlInvalidated()
    {
        _anyControlInvalid = true;
    }

    private void Refresh(IControl control)
    {
        // Just draw the control - don't invalidate it.
        // The control should already be marked as invalid if it needs redrawing.
        control.Refresh(_graphics);
    }

    /// <summary>
    /// Calculates the bounding rectangle that encompasses all invalid controls in a single pass
    /// </summary>
    private bool TryGetDirtyRegion(out int left, out int top, out int right, out int bottom)
    {
        left = int.MaxValue;
        top = int.MaxValue;
        right = int.MinValue;
        bottom = int.MinValue;

        bool foundInvalid = CollectDirtyBoundsRecursive(Controls, ref left, ref top, ref right, ref bottom);

        if (!foundInvalid)
        {
            left = top = right = bottom = 0;
            return false;
        }

        // Expand dirty region by 2 pixels in each direction to ensure edge decorations
        // (like underlines) are included, especially important with rotation and alignment
        left = Math.Max(0, left - 2);
        top = Math.Max(0, top - 2);
        right = Math.Min(Width, right + 2);
        bottom = Math.Min(Height, bottom + 2);

        return true;
    }

    /// <summary>
    /// Recursively collects dirty bounds from invalid controls in a single pass
    /// </summary>
    private bool CollectDirtyBoundsRecursive(ControlsCollection controls, ref int left, ref int top, ref int right, ref int bottom)
    {
        bool foundInvalid = false;

        foreach (var control in controls)
        {
            // Include invalid controls in dirty region regardless of visibility
            // This ensures invisible controls get their area cleared
            if (control.IsInvalid)
            {
                foundInvalid = true;

                // Update bounds in single pass
                if (control.ScreenLeft < left) left = control.ScreenLeft;
                if (control.ScreenTop < top) top = control.ScreenTop;
                if (control.ScreenRight > right) right = control.ScreenRight;
                if (control.ScreenBottom > bottom) bottom = control.ScreenBottom;
            }

            // Handle nested containers - only recurse if the container is visible
            // This prevents children of invisible containers from being included
            if (control is IControlContainer container && control.IsVisible)
            {
                if (CollectDirtyBoundsRecursive(container.Controls, ref left, ref top, ref right, ref bottom))
                {
                    foundInvalid = true;
                }
            }
        }

        return foundInvalid;
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
            // Include invisible controls only if they're invalid (need their area cleared)
            // Skip invisible valid controls as they don't need processing
            if (!control.IsVisible && !control.IsInvalid) continue;

            // Collect ALL controls that intersect the dirty region, not just invalid ones
            // We need to redraw valid controls too because we clear the entire dirty region
            if (IntersectsRegion(control, left, top, right, bottom))
            {
                controlsToRedraw.Add(control);
            }

            // Recursively check children - only if container is visible
            if (control is IControlContainer container && control.IsVisible)
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
            if (!_updateInProgress && (IsInvalid || _anyControlInvalid))
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
                        Resolver.Log.Warn($"MicroGraphics.Show error while drawing screen: {ex.Message} : {ex.StackTrace}", "MicroLayout");
                    }
                }
                else if (TryGetDirtyRegion(out int left, out int top, out int right, out int bottom))
                {
                    // Partial screen invalidation - only update dirty region
                    int width = right - left;
                    int height = bottom - top;

                    // Collect all controls that intersect with the dirty region (including overlapping ones)
                    _reusableControlList.Clear();
                    CollectControlsInRegion(Controls, left, top, right, bottom, _reusableControlList);

                    // Clear the background for invalid controls only (those that are changing)
                    // This prevents ghosting without erasing valid overlapping controls
                    foreach (var control in _reusableControlList)
                    {
                        if (control.IsInvalid)
                        {
                            // Clear this control's area (whether visible or invisible)
                            // Invisible controls need their area cleared to remove old pixels
                            _graphics.DrawRectangle(control.ScreenLeft, control.ScreenTop,
                                control.Width, control.Height, BackgroundColor, true);
                        }
                        else if (control.IsVisible)
                        {
                            // Mark all visible valid controls in the dirty region as invalid so they redraw
                            // This ensures that valid controls that overlap with invalid controls get redrawn
                            control.Invalidate();
                        }
                    }
                    foreach (var control in _reusableControlList)
                    {
                        // Redraw all controls in the dirty region
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
                        Resolver.Log.Warn($"MicroGraphics.Show({left}, {top}, {right}, {bottom}) error while drawing screen: {ex.Message} : {ex.StackTrace}", "MicroLayout");
                    }
                }

                IsInvalid = false;
                _anyControlInvalid = false;
            }
        }
    }

    private void DrawLoopOnCaller()
    { // this loop is used by platforms where drawing can happen on any thread (e.g. meadow, or Linux with a SPI display)
        while (true)
        {
            if (!_updateInProgress && (IsInvalid || _anyControlInvalid))
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