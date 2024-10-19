using FTD2XX;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a collection of FtdiExpander devices connected to the host machine.
/// </summary>
public class FtdiExpanderCollection : IEnumerable<FtdiExpander>
{
    private static FtdiExpanderCollection? _instance;

    private List<FtdiExpander> _expanders = new List<FtdiExpander>();

    /// <summary>
    /// Gets the number of FtdiExpander devices connected to the host machine.
    /// </summary>
    public int Count => _expanders.Count;

    /// <summary>
    /// Gets the FtdiExpander device at the specified index in the collection.
    /// </summary>
    /// <param name="index">The index of the FtdiExpander device to retrieve.</param>
    public FtdiExpander this[int index] => _expanders[index];

    private FtdiExpanderCollection()
    {
    }

    /// <summary>
    /// Refresh the collection of FtdiExpander devices connected to the host machine.
    /// </summary>
    public void Refresh()
    {
        // the FTDI class is poorly designed.  It holdsan internal handle, but also can be used to query globals
        // until I rewrite it, we'll use this hack/trash
        var api = new FTDI();

        var deviceCount = 0;
        api.GetNumberOfDevices(ref deviceCount).ThrowIfNotOK();
        var deviceInfos = new FT_DEVICE_INFO_NODE[deviceCount];
        api.GetDeviceList(deviceInfos).ThrowIfNotOK();

        _expanders.Clear();

        ReadOnlySpan<byte> serialNumberBuffer = stackalloc byte[16];
        ReadOnlySpan<byte> descriptionBuffer = stackalloc byte[64];

        for (int index = 0; index < deviceCount; index++)
        {
            var device = new FTDI(); // create a new instance that will hold our handle
            device.OpenByIndex(index);

            FT_DEVICE type = FT_DEVICE.FT_DEVICE_UNKNOWN;

            device.GetDeviceType(ref type);

            switch (type)
            {
                case FT_DEVICE.FT_DEVICE_232H:
                case FT_DEVICE.FT_DEVICE_2232H:
                case FT_DEVICE.FT_DEVICE_4232H:
                case FT_DEVICE.FT_DEVICE_2232:
                    // valid, add to list
                    break;
                default:
                    continue;
            }

            device.GetSerialNumber(out string serialNumber);
            device.GetDescription(out string description);

            device.Close();

            _expanders.Add(FtdiExpander.Create(device, index, type, serialNumber, description));
        }
    }

    /// <inheritdoc/>
    public IEnumerator<FtdiExpander> GetEnumerator()
    {
        return _expanders.GetEnumerator();
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
