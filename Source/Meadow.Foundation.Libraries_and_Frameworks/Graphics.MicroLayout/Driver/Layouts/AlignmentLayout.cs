using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics.MicroLayout;

/// <summary>
/// A layout that arranges child controls based on a docking position.
/// </summary>
public class AlignmentLayout : LayoutBase
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
    public int Padding
    {
        get => _padding;
        set
        {
            if (_padding == value) { return; }
            _padding = value;
            Invalidate();
        }
    }

    private int _padding = 2;

    private readonly Dictionary<IControl, DockPosition> _dockPositions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignmentLayout"/> class.
    /// </summary>
    /// <param name="left">The left position of the layout.</param>
    /// <param name="top">The top position of the layout.</param>
    /// <param name="width">The width of the layout.</param>
    /// <param name="height">The height of the layout.</param>
    public AlignmentLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        Controls.ControlAdded += OnControlsChanged;
        Controls.ControlRemoved += OnControlsChanged;
    }

    private void OnControlsChanged(object sender, IControl e)
    {
        Invalidate();
    }

    /// <summary>
    /// Adds a control to the layout at the specified docking position.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <param name="position">The docking position for the control.</param>
    public void Add(IControl control, DockPosition position)
    {
        _dockPositions[control] = position;
        Controls.Add(control);
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
    /// Note: This layout does not "consume" space like a traditional DockPanel.
    /// Multiple controls at the same position will overlap.
    /// </summary>
    /// <param name="control">The control to arrange.</param>
    /// <param name="position">The docking position of the control.</param>
    private void SetControlPosition(IControl control, DockPosition position)
    {
        int contentX = Padding;
        int contentY = Padding;
        int contentWidth = Width - (2 * Padding);
        int contentHeight = Height - (2 * Padding);

        if (contentWidth < 0) contentWidth = 0;
        if (contentHeight < 0) contentHeight = 0;

        int controlX = 0;
        int controlY = 0;

        switch (position)
        {
            case DockPosition.Top:
                controlX = contentX + (contentWidth / 2) - (control.Width / 2);
                controlY = contentY;
                break;
            case DockPosition.Bottom:
                controlX = contentX + (contentWidth / 2) - (control.Width / 2);
                controlY = contentY + contentHeight - control.Height;
                break;
            case DockPosition.Left:
                controlX = contentX;
                controlY = contentY + (contentHeight / 2) - (control.Height / 2);
                break;
            case DockPosition.Right:
                controlX = contentX + contentWidth - control.Width;
                controlY = contentY + (contentHeight / 2) - (control.Height / 2);
                break;
            case DockPosition.TopLeft:
                controlX = contentX;
                controlY = contentY;
                break;
            case DockPosition.TopRight:
                controlX = contentX + contentWidth - control.Width;
                controlY = contentY;
                break;
            case DockPosition.BottomLeft:
                controlX = contentX;
                controlY = contentY + contentHeight - control.Height;
                break;
            case DockPosition.BottomRight:
                controlX = contentX + contentWidth - control.Width;
                controlY = contentY + contentHeight - control.Height;
                break;
            case DockPosition.Center:
                controlX = contentX + (contentWidth / 2) - (control.Width / 2);
                controlY = contentY + (contentHeight / 2) - (control.Height / 2);
                break;
        }

        control.Left = controlX;
        control.Top = controlY;
    }

    /// <InheritDoc/>
    internal override void PerformLayout()
    {
        lock (Controls.SyncRoot)
        {
            foreach (var control in Controls)
            {
                if (_dockPositions.TryGetValue(control, out DockPosition position))
                {
                    SetControlPosition(control, position);
                }
            }
        }
    }
}