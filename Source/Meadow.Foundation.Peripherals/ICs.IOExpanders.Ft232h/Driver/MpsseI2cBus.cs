using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders;

public sealed class MpsseI2cBus : IFt232Bus, II2cBus, IDisposable
{
    private const byte DefaultLatencyTimer = 10;
    private const I2CChannelOptions DefaultChannelOptions = I2CChannelOptions.None;

    private bool _isDisposed;

    public IntPtr Handle { get; private set; }
    public byte GpioDirectionMask { get; set; }
    public byte GpioState { get; set; }
    internal bool IsOpen { get; private set; } = false;
    internal int ChannelNumber { get; }
    private FT_DEVICE_LIST_INFO_NODE InfoNode { get; }

    internal MpsseI2cBus(int channelNumber, FT_DEVICE_LIST_INFO_NODE info)
    {
        ChannelNumber = channelNumber;
        InfoNode = info;
    }

    public I2cBusSpeed BusSpeed { get; set; }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            CloseChannel();

            _isDisposed = true;
        }
    }

    ~MpsseI2cBus()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal void Open(I2CClockRate clockRate = I2CClockRate.Standard)
    {
        if (CheckStatus(Mpsse.I2C_OpenChannel(ChannelNumber, out IntPtr handle)))
        {
            Handle = handle;

            var config = new I2CChannelConfig
            {
                ClockRate = clockRate,
                LatencyTimer = DefaultLatencyTimer,
                Options = DefaultChannelOptions
            };

            CheckStatus(Mpsse.I2C_InitChannel(Handle, ref config));

            IsOpen = true;
        }
    }

    private void CloseChannel()
    {
        if (Handle != IntPtr.Zero)
        {
            CheckStatus(Mpsse.I2C_CloseChannel(Handle));
            Handle = IntPtr.Zero;
        }
    }

    public void Exchange(byte peripheralAddress, Span<byte> writeBuffer, Span<byte> readBuffer)
    {
        Write(peripheralAddress, writeBuffer);
        Read(peripheralAddress, readBuffer);
    }

    public void Read(byte peripheralAddress, Span<byte> readBuffer)
    {
        var status = Mpsse.I2C_DeviceRead(
            Handle,
            peripheralAddress,
            readBuffer.Length,
            MemoryMarshal.GetReference(readBuffer),
            out int transferred,
            I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.FAST_TRANSFER_BYTES
            //I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.NACK_LAST_BYTE
            //                I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.NACK_LAST_BYTE
            );

        Debug.WriteLine($"transferred: {transferred}");
        CheckStatus(status);
    }

    public void Write(byte peripheralAddress, Span<byte> writeBuffer)
    {
        var status = Mpsse.I2C_DeviceWrite(
            Handle,
            peripheralAddress,
            writeBuffer.Length,
            MemoryMarshal.GetReference(writeBuffer),
            out int transferred,
                            I2CTransferOptions.FAST_TRANSFER | I2CTransferOptions.FAST_TRANSFER_BYTES
            //I2CTransferOptions.START_BIT | I2CTransferOptions.BREAK_ON_NACK
            //I2CTransferOptions.START_BIT | I2CTransferOptions.STOP_BIT | I2CTransferOptions.NACK_LAST_BYTE
            );

        Debug.WriteLine($"transferred: {transferred}");
        //            CheckStatus(status);
    }
}