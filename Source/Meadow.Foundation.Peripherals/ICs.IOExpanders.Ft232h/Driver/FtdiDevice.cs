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

    private void InitializeClocks()
    {
        // Now setup the clock and other elements
        Span<byte> toSend = stackalloc byte[13];
        int idx = 0;
        // Disable clock divide by 5 for 60Mhz master clock
        toSend[idx++] = (byte)FT_OPCODE.DisableClockDivideBy5;
        // Turn off adaptive clocking
        toSend[idx++] = (byte)FT_OPCODE.TurnOffAdaptiveClocking;
        // Enable 3 phase data clock, used by I2C to allow data on both clock edges
        toSend[idx++] = (byte)FT_OPCODE.Enable3PhaseDataClocking;
        // The SK clock frequency can be worked out by below algorithm with divide by 5 set as off
        // TCK period = 60MHz / (( 1 + [ (0xValueH * 256) OR 0xValueL] ) * 2)
        // Command to set clock divisor
        toSend[idx++] = (byte)FT_OPCODE.SetClockDivisor;
        uint clockDivisor = (60000 / (I2cBusFrequencyKbps * 2)) - 1;
        toSend[idx++] = (byte)(clockDivisor & 0x00FF);
        toSend[idx++] = (byte)((clockDivisor >> 8) & 0x00FF);
        // loopback off
        toSend[idx++] = (byte)FT_OPCODE.DisconnectTDItoTDOforLoopback;
        // Enable the FT232H's drive-zero mode with the following enable mask
        toSend[idx++] = (byte)FT_OPCODE.SetIOOnlyDriveOn0AndTristateOn1;
        // Low byte (ADx) enables - bits 0, 1 and 2
        toSend[idx++] = 0x07;
        // High byte (ACx) enables - all off
        toSend[idx++] = 0x00;
        // Command to set directions of lower 8 pins and force value on bits set as output
        toSend[idx++] = (byte)FT_OPCODE.SetDataBitsLowByte;
        if (DeviceType == FtDeviceType.Ft232H)
        {
            // SDA and SCL both output high(open drain)
            GpioLowData = (byte)(PinData.SDAhiSCLhi | (GpioLowData & MaskGpio));
            GpioLowDir = (byte)(PinDirection.SDAoutSCLout | (GpioLowDir & MaskGpio));
        }
        else
        {
            // SDA and SCL set low but as input to mimic open drain
            GpioLowData = (byte)(PinData.SDAloSCLlo | (GpioLowData & MaskGpio));
            GpioLowDir = (byte)(PinDirection.SDAinSCLin | (GpioLowDir & MaskGpio));
        }

        toSend[idx++] = GpioLowData;
        toSend[idx++] = GpioLowDir;
        Write(toSend);
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
