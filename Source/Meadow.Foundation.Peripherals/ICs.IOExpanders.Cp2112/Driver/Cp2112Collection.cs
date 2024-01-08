using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a collection of Cp2112 devices and provides functionality for device enumeration.
/// </summary>
public class Cp2112Collection : IEnumerable<Cp2112>
{
    private static Cp2112Collection? _instance;

    private List<Cp2112> _list = new List<Cp2112>();

    /// <summary>
    /// Gets the number of Cp2112 devices connected to the host machine.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the Cp2112 device at the specified index in the collection.
    /// </summary>
    /// <param name="index">The index of the Cp2112 device to retrieve.</param>
    public Cp2112 this[int index] => _list[index];

    private Cp2112Collection()
    {
    }

    /// <summary>
    /// Refreshes the collection by detecting and updating Cp2112 devices.
    /// </summary>
    public void Refresh()
    {
        _list.Clear();

        uint deviceCount = 0;

        var vid = Native.UsbParameters.SG_VID;
        var pid = Native.UsbParameters.CP2112_PID;

        Native.CheckStatus(Native.Functions.HidSmbus_GetNumDevices(ref deviceCount, vid, pid));

        for (var i = 0; i < deviceCount; i++)
        {
            _list.Add(new Cp2112(i, vid, pid));
        }

    }

    /// <inheritdoc/>
    public IEnumerator<Cp2112> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the singleton instance of Cp2112Collection, initializing it if necessary.
    /// </summary>
    public static Cp2112Collection Devices
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Cp2112Collection();
                _instance.Refresh();
            }
            return _instance;
        }
    }
}