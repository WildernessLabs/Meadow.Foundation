using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Meadow.Foundation.ICs.IOExpanders;

internal class MpsseChannelCollection : IEnumerable<MpsseChannel>
{
    private List<MpsseChannel> _devices = new();

    public int Count => _devices.Count;

    public MpsseChannel this[int index]
    {
        get => _devices[index];
    }

    public void Refresh()
    {
        _devices.Clear();

        uint count;

        try
        {
            Native.CheckStatus(Native.Mpsse.FT_CreateDeviceInfoList(out count));
        }
        catch (DllNotFoundException)
        {
            throw new DriverNotInstalledException();
        }

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

            _devices.Add(new MpsseChannel(index, flags, deviceType, id, locid, serialNumber, description, handle));
        }
    }

    public IEnumerator<MpsseChannel> GetEnumerator()
    {
        return _devices.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
