using System;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

public class FtdiExpanderCollection : IEnumerable<FtdiExpander>
{
    private FTDI _native;

    private static FtdiExpanderCollection? _instance;

    private List<FtdiExpander> _list = new List<FtdiExpander>();

    /// <summary>
    /// Gets the number of FtdiExpander devices connected to the host machine.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the FtdiExpander device at the specified index in the collection.
    /// </summary>
    /// <param name="index">The index of the FtdiExpander device to retrieve.</param>
    public FtdiExpander this[int index] => _list[index];

    private FtdiExpanderCollection()
    {
        _native = new FTDI();
    }

    private void CheckStatus(FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK) return;

        throw new Exception($"ftd2xx.dll returned '{status}'");
    }

    public void Refresh()
    {
        uint count = 0;

        CheckStatus(_native.GetNumberOfDevices(ref count));

        _list.Clear();

        if (count > 0)
        {
            var infos = new FTDI.FT_DEVICE_INFO_NODE[count];
            CheckStatus(_native.GetDeviceList(infos));

            foreach (var info in infos)
            {
                _list.Add(FtdiExpander.From(info, _native));
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<FtdiExpander> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the singleton instance of Ft232Collection, initializing it if necessary.
    /// </summary>
    public static FtdiExpanderCollection Devices
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FtdiExpanderCollection();
                _instance.Refresh();
            }
            return _instance;
        }
    }
}
