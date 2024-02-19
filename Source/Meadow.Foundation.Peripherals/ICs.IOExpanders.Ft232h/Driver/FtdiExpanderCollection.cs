using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

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

    public void Refresh()
    {
        Native.CheckStatus(
            Native.Ftd2xx.FT_CreateDeviceInfoList(out uint count));

        _expanders.Clear();

        ReadOnlySpan<byte> serialNumberBuffer = stackalloc byte[16];
        ReadOnlySpan<byte> descriptionBuffer = stackalloc byte[64];

        for (uint index = 0; index < count; index++)
        {
            Native.CheckStatus(FT_GetDeviceInfoDetail(
                index,
                out uint flags,
                out FtDeviceType deviceType,
                out uint id,
                out uint locid,
                in MemoryMarshal.GetReference(serialNumberBuffer),
                in MemoryMarshal.GetReference(descriptionBuffer),
                out IntPtr handle));

            switch (deviceType)
            {
                case FtDeviceType.Ft232H:
                case FtDeviceType.Ft2232:
                case FtDeviceType.Ft2232H:
                case FtDeviceType.Ft4232H:
                    // valid, add to list
                    break;
                default:
                    continue;
            }

            // no idea why the buffer isn't all zeros after the null terminator - thanks FTDI!
            var serialNumber = Encoding.ASCII.GetString(serialNumberBuffer.ToArray(), 0, serialNumberBuffer.IndexOf((byte)0));
            var description = Encoding.ASCII.GetString(descriptionBuffer.ToArray(), 0, descriptionBuffer.IndexOf((byte)0));

            _expanders.Add(FtdiExpander.Create(index, flags, deviceType, id, locid, serialNumber, description, handle));
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
