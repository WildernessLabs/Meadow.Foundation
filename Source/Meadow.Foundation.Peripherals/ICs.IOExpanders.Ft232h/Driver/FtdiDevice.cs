using System;
using System.IO;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Native;
using static Meadow.Foundation.ICs.IOExpanders.Native.Ftd2xx;

namespace Meadow.Foundation.ICs.IOExpanders;

internal partial class FtdiDevice
{
    private const int DefaultTimeoutMs = 5000;
    private const int DefaultLatencyTimer = 16;

    internal uint Index { get; }
    internal uint Flags { get; }
    internal FtDeviceType DeviceType { get; }
    internal uint ID { get; }
    internal uint LocID { get; }
    internal string SerialNumber { get; }
    internal string Description { get; }
    internal IntPtr Handle { get; private set; }

    internal FtdiDevice(
        uint index,
        uint flags,
        FtDeviceType deviceType,
        uint id,
        uint locid,
        string serialNumber,
        string description,
        IntPtr handle
        )
    {
        Index = index;
        Flags = flags;
        DeviceType = deviceType;
        ID = id;
        LocID = locid;
        SerialNumber = serialNumber;
        Description = description;
        Handle = handle;
    }

    public void Open()
    {
        if (Handle == IntPtr.Zero)
        {
            Native.CheckStatus(
                FT_OpenEx(LocID, Native.FT_OPEN_TYPE.FT_OPEN_BY_LOCATION, out IntPtr handle)
                );
            Handle = handle;
        }
    }

    private void InitializeMpsse()
    {
        Span<byte> writeBuffer = stackalloc byte[1];
        writeBuffer[0] = 0xAA;
        Write(writeBuffer);
        Span<byte> readBuffer = stackalloc byte[2];
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAA)))
        {
            throw new IOException($"Failed to setup device {Description} in MPSSE mode using magic 0xAA sync");
        }

        // Second with 0xAB
        writeBuffer[0] = 0xAB;
        Write(writeBuffer);
        ReadInto(readBuffer);
        if (!((readBuffer[0] == 0xFA) && (readBuffer[1] == 0xAB)))
        {
            throw new IOException($"Failed to setup device {Description}, status in MPSSE mode using magic 0xAB sync");
        }
    }

    public int ReadInto(Span<byte> buffer)
    {
        var totalRead = 0;
        uint read = 0;

        while (totalRead < buffer.Length)
        {
            var available = GetAvailableBytes();
            if (available > 0)
            {
                CheckStatus(
                    FT_Read(Handle, in buffer[totalRead], available, ref read));

                totalRead += (int)read;
            }
        }

        return totalRead;
    }

    public void Write(ReadOnlySpan<byte> data)
    {
        uint written = 0;

        CheckStatus(
            FT_Write(Handle, in MemoryMarshal.GetReference(data), (ushort)data.Length, ref written));
    }

    private void ClearInputBuffer()
    {
        var available = GetAvailableBytes();

        if (available > 0)
        {
            var toRead = new byte[available];
            uint bytesRead = 0;
            CheckStatus(
                FT_Read(Handle, in toRead[0], available, ref bytesRead));
        }
    }

    private uint GetAvailableBytes()
    {
        uint availableBytes = 0;

        CheckStatus(
            FT_GetQueueStatus(Handle, ref availableBytes));

        return availableBytes;
    }

    public void Close()
    {
        if (Handle != IntPtr.Zero)
        {
            CheckStatus(
                FT_Close(Handle));

            Handle = IntPtr.Zero;
        }
    }
}
