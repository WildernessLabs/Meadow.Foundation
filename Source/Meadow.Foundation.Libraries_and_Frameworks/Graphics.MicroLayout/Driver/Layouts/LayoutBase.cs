using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A base class for display layouts
/// </summary>
public abstract class LayoutBase : ThemedControl, ILayout
{
    /// <summary>
    /// Gets the collection of controls on the display screen.
    /// </summary>
    public virtual ControlsCollection Controls { get; private set; }

    /// <inheritdoc/>
    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            Invalidate();
        }
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
    public override bool IsInvalid => base.IsInvalid || Controls.Any(c => c.IsInvalid && c.IsVisible);

    private Color? _backColor;

    /// <summary>
    /// Creates a LayoutBase
    /// </summary>
    protected LayoutBase() : this(0, 0, 0, 0) { }

    /// <summary>
    /// Creates a LayoutBase
    /// </summary>
    /// <param name="left">The layout's left position</param>
    /// <param name="top">The layout's top position</param>
    /// <param name="width">The layout's width</param>
    /// <param name="height">The layout's height</param>
    protected LayoutBase(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        Controls = new ControlsCollection(this);
        Controls.ControlAdded += OnChildrenCollectionChanged;
        Controls.ControlRemoved += OnChildrenCollectionChanged;
    }

    /// <summary>
    /// Performs layout of the controls within the layout
    /// </summary>
    internal abstract void PerformLayout();

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls.OfType<IThemedControl>())
            {
                control.ApplyTheme(theme);
            }

            if (theme.BackgroundColor != null)
            {
                BackgroundColor = theme.BackgroundColor;
            }
        }
        Invalidate();
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

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (!IsVisible) { return; }

        if (BackgroundColor != null && BackgroundColor != Color.Transparent)
        {
            graphics.DrawRectangle(ScreenLeft, ScreenTop, Width, Height, BackgroundColor.Value, true);
        }

        foreach (var control in Controls)
        {
            control.Refresh(graphics);
        }
    }

    /// <inheritdoc />
    protected override void OnParentBoundsChanged(object? sender, EventArgs e)
    {
        base.OnParentBoundsChanged(sender, e);
        PerformLayout();
    }

    /// <inheritdoc />
    protected virtual void OnChildrenCollectionChanged(object? sender, IControl control)
    {
        PerformLayout();
        Invalidate();
    }
}