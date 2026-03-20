using System;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// Represents a progress bar control in the user interface.
/// </summary>
public class ProgressBar : ThemedControl
{
    private Color _valueColor = Color.DarkBlue;
    private Color _borderColor = Color.Transparent;
    private Color _backgroundColor = Color.DarkGray;
    private int _value = 0;
    private int _minimum = 0;
    private int _maximum = 100;

    /// <summary>
    /// Represents a rectangular, horizontal progress bar in the user interface.
    /// </summary>
    /// 
    public ProgressBar(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <inheritdoc/>
    public override void ApplyTheme(DisplayTheme theme)
    {
        if (theme != null)
        {
            if (theme.ForegroundColor != null) ValueColor = theme.ForegroundColor.Value;
            if (theme.BackgroundColor != null) BackgroundColor = theme.BackgroundColor.Value;
            if (theme.HighlightColor != null) BorderColor = theme.HighlightColor.Value;
        }
    }

    /// <summary>
    /// Gets or set the Value for the ProgressBar
    /// </summary>
    public int Value
    {
        get => _value;
        set
        {
            if (value > Maximum || value < Minimum) throw new ArgumentOutOfRangeException();
            SetInvalidatingProperty(ref _value, value);
        }
    }

    /// <summary>
    /// Gets or set the minimum Value for the ProgressBar
    /// </summary>
    public int Minimum
    {
        get => _minimum;
        set
        {
            if (value >= Maximum) throw new ArgumentOutOfRangeException();
            SetInvalidatingProperty(ref _minimum, value);
        }
    }

    /// <summary>
    /// Gets or set the maximum Value for the ProgressBar
    /// </summary>
    public int Maximum
    {
        get => _maximum;
        set
        {
            if (value <= Minimum) throw new ArgumentOutOfRangeException();
            SetInvalidatingProperty(ref _maximum, value);
        }
    }

    /// <summary>
    /// Gets or sets the foreground (value) color to fill on the ProgressBar
    /// </summary>
    public Color ValueColor
    {
        get => _valueColor;
        set => SetInvalidatingProperty(ref _valueColor, value);
    }

    /// <summary>
    /// Gets or sets the background (non-value) color to fill on the ProgressBar
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set => SetInvalidatingProperty(ref _backgroundColor, value);
    }

    /// <summary>
    /// Gets or sets the border color to around the ProgressBar
    /// </summary>
    public Color BorderColor
    {
        get => _borderColor;
        set => SetInvalidatingProperty(ref _borderColor, value);
    }

    /// <inheritdoc/>
    protected override void OnDraw(MicroGraphics graphics)
    {
        var valueWidth = (int)((Value / (float)(Maximum - Minimum)) * Width);
        var emptyWidth = Width - valueWidth;

        if (ValueColor != Color.Transparent)
        {
            graphics.DrawRectangle(
                ScreenLeft,
                ScreenTop,
                valueWidth,
                Height,
                ValueColor,
                true);
        }
        if (BackgroundColor != Color.Transparent)
        {
            graphics.DrawRectangle(
                ScreenLeft + valueWidth,
                ScreenTop,
                emptyWidth,
                Height,
                BackgroundColor,
                true);
        }
        if (BorderColor != Color.Transparent)
        {
            graphics.DrawRectangle(
                ScreenLeft,
                ScreenTop,
                Width,
                Height,
                BorderColor,
                false);
        }
    }
}
