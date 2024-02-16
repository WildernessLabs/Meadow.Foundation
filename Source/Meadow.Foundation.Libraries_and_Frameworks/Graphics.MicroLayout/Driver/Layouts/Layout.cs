﻿using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A base class for display layouts
/// </summary>
public abstract class Layout : ThemedControl
{
    private Color? _backColor;

    /// <summary>
    /// Creates a DisplayLayout
    /// </summary>
    /// <param name="screen">The DisplayScreen to associate this layout with</param>
    /// <param name="left">The layout's left position</param>
    /// <param name="top">The layout's top position</param>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    protected Layout(DisplayScreen screen, int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        Controls = new ControlsCollection(screen, this);
    }

    /// <summary>
    /// Gets or sets the background color of the Layout.
    /// </summary>
    public Color? BackgroundColor
    {
        get => _backColor;
        set => SetInvalidatingProperty(ref _backColor, value);
    }

    /// <inheritdoc/>
    public override bool IsInvalid => base.IsInvalid || Controls.Any(c => c.IsInvalid);

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        lock (Controls.SyncRoot)
        {
            foreach (IThemedControl control in Controls)
            {
                control.ApplyTheme(theme);
            }
        }
    }

    /// <inheritdoc/>
    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            foreach (var control in Controls)
            {
                control.IsVisible = value;
            }
            base.IsVisible = value;
        }
    }

    /// <inheritdoc/>
    public override void Invalidate()
    {
        if (Controls == null) return;

        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                control.Invalidate();
            }
        }
        base.Invalidate();
    }

    /// <summary>
    /// Gets the collection of controls on the display screen.
    /// </summary>
    public ControlsCollection Controls { get; private set; }
}
