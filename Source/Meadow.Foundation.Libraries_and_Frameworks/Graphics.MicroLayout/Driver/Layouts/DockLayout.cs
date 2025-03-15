using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls based on a docking position.
/// </summary>
public class DockLayout : MicroLayout
{
    /// <summary>
    /// Specifies the docking position of a control within the layout.
    /// </summary>
    public enum DockPosition
    {
        /// <summary>
        /// Positions the control at the top of the layout.
        /// </summary>
        Top,
        /// <summary>
        /// Positions the control at the bottom of the layout.
        /// </summary>
        Bottom,
        /// <summary>
        /// Positions the control on the left side of the layout.
        /// </summary>
        Left,
        /// <summary>
        /// Positions the control on the right side of the layout.
        /// </summary>
        Right,
        /// <summary>
        /// Positions the control at the top-left corner of the layout.
        /// </summary>
        TopLeft,
        /// <summary>
        /// Positions the control at the top-right corner of the layout.
        /// </summary>
        TopRight,
        /// <summary>
        /// Positions the control at the bottom-left corner of the layout.
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Positions the control at the bottom-right corner of the layout.
        /// </summary>
        BottomRight,
        /// <summary>
        /// Positions the control at the center of the layout.
        /// </summary>
        Center
    }

    /// <summary>
    /// Gets or sets the padding around the controls in the layout.
    /// </summary>
    public int Padding { get; set; } = 2;

    private readonly Dictionary<IControl, DockPosition> _dockPositions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DockLayout"/> class.
    /// </summary>
    /// <param name="left">The left position of the layout.</param>
    /// <param name="top">The top position of the layout.</param>
    /// <param name="width">The width of the layout.</param>
    /// <param name="height">The height of the layout.</param>
    public DockLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
    }

    /// <summary>
    /// Adds a control to the layout at the specified docking position.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <param name="position">The docking position for the control.</param>
    public void Add(IControl control, DockPosition position)
    {
        Controls.Add(control);
        _dockPositions[control] = position;

        SetControlPosition(control, position);
    }

    /// <summary>
    /// Removes a control from the layout.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    public void Remove(IControl control)
    {
        if (Controls.Contains(control))
        {
            Controls.Remove(control);
            _dockPositions.Remove(control);
        }
    }

    /// <summary>
    /// Arranges the layout of the specified control based on its docking position.
    /// </summary>
    /// <param name="control">The control to arrange.</param>
    /// <param name="position">The docking position of the control.</param>
    private void SetControlPosition(IControl control, DockPosition position)
    {
        switch (position)
        {
            case DockPosition.Top:
                control.Left = base.Width / 2 - control.Width / 2;
                control.Top = Padding;
                break;
            case DockPosition.Bottom:
                control.Left = Width / 2 - control.Width / 2;
                control.Top = Height - Padding - control.Height;
                break;
            case DockPosition.Left:
                control.Left = Padding;
                control.Top = Height / 2 - control.Height / 2;
                break;
            case DockPosition.Right:
                control.Left = Width - control.Width - Padding;
                control.Top = Height / 2 - control.Height / 2;
                break;
            case DockPosition.TopLeft:
                control.Left = Padding;
                control.Top = Padding;
                break;
            case DockPosition.TopRight:
                control.Left = Width - control.Width - Padding;
                control.Top = Padding;
                break;
            case DockPosition.BottomLeft:
                control.Left = Padding;
                control.Top = Height - control.Height - Padding;
                break;
            case DockPosition.BottomRight:
                control.Left = Width - control.Width - Padding;
                control.Top = Height - control.Height - Padding;
                break;
            case DockPosition.Center:
                control.Left = Width / 2 - control.Width / 2;
                control.Top = Height / 2 - control.Height / 2;
                break;
        }
    }

    /// <summary>
    /// Draws the layout using the specified graphics context.
    /// </summary>
    /// <param name="graphics">The graphics context used to render the layout.</param>
    protected override void OnDraw(MicroGraphics graphics)
    {
        if (!IsVisible || BackgroundColor == null)
            return;

        graphics.DrawRectangle(
            Left + (Parent?.Left ?? 0),
            Top + (Parent?.Top ?? 0),
            Width,
            Height,
            BackgroundColor.Value,
            true);
    }
}
