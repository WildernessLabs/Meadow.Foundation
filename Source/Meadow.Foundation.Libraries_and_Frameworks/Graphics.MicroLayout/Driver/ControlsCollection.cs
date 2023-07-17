using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Displays.UI;

public sealed class ControlsCollection : IEnumerable<IDisplayControl>
{
    private DisplayScreen _screen;
    private List<IDisplayControl> _controls = new();

    internal ControlsCollection(DisplayScreen screen)
    {
        _screen = screen;
    }

    public void Clear()
    {
        _controls.Clear();

        // _screen.Invalidate();  // TODO?
    }

    public int Count => _controls.Count;

    public void Add(params IDisplayControl[] controls)
    {
        // apply screen theme
        if (_screen.Theme != null)
        {
            foreach (var control in controls)
            {
                control.ApplyTheme(_screen.Theme);
            }
        }

        _controls.AddRange(controls);
    }

    public IEnumerator<IDisplayControl> GetEnumerator()
    {
        return _controls.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
